import { useState, useEffect } from "react";
import { Link, useNavigate, useSearch } from "@tanstack/react-router";
import { usePipelineNodeMutations, useCustomNodeById } from "../hooks/usePipelines";
import { NodeMetaForm } from "../components/node-editor/NodeMetaForm";
import { ScriptUploadBox } from "../components/node-editor/ScriptUploadBox";
import { PinCardList } from "../components/node-editor/PinCardList";
import { PinConfigInspector } from "../components/node-editor/PinConfigInspector";
import { uploadAssetFlow } from "@/lib/upload-utils";
import type { PinDefinition } from "@/gen/model";
import { PinPrimitiveType } from "@/gen/model/pinPrimitiveType";
import { PinCardinality } from "@/gen/model/pinCardinality";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Save, Loader2, Workflow } from "lucide-react";
import { toast } from "sonner";

interface CreateCustomNodePageProps {
  projectId: string;
}

export function CreateCustomNodePage({ projectId }: CreateCustomNodePageProps) {
  const navigate = useNavigate();
  const search = useSearch({ strict: false }) as any;
  const editNodeId = search?.editNodeId as string | undefined;

  const { parseScript, createNode, isCreatingNode, updateNode, isUpdatingNode } =
    usePipelineNodeMutations(projectId);

  const { data: existingNode, isLoading: isLoadingExisting } = useCustomNodeById(editNodeId);

  const [name, setName] = useState("");
  const [label, setLabel] = useState("");
  const [executor, setExecutor] = useState<"blender" | "python" | "unreal">("blender");
  const [scriptContent, setScriptContent] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadedAssetId, setUploadedAssetId] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [isDetecting, setIsDetecting] = useState(false);

  const [inputs, setInputs] = useState<PinDefinition[]>([]);
  const [outputs, setOutputs] = useState<PinDefinition[]>([]);

  // Load existing node data when editing
  useEffect(() => {
    if (existingNode) {
      setName(existingNode.name || "");
      setLabel(existingNode.label || existingNode.name || "");
      if (existingNode.executor === "blender" || existingNode.executor === "python" || existingNode.executor === "unreal") {
        setExecutor(existingNode.executor);
      }
      if (existingNode.inputs) setInputs(existingNode.inputs);
      if (existingNode.outputs) setOutputs(existingNode.outputs);
    }
  }, [existingNode]);

  const [selectedPin, setSelectedPin] = useState<{
    pin: PinDefinition;
    direction: "in" | "out";
    index: number;
  } | null>(null);

  const handleFileSelect = async (file: File) => {
    setSelectedFile(file);
    const text = await file.text();
    setScriptContent(text);
    autoDetectFromScript(text, file.name);
  };

  const autoDetectFromScript = async (code: string, fileName?: string) => {
    if (!code.trim()) {
      toast.error("Please provide Python script code to analyze.");
      return;
    }

    try {
      setIsDetecting(true);
      const res = await parseScript({
        scriptContent: code,
        fileName: fileName || selectedFile?.name || "CustomNode.py",
      });

      if (res) {
        if (!name && res.suggestedName) setName(res.suggestedName);
        if (!label && res.suggestedLabel) setLabel(res.suggestedLabel);
        if (res.executor === "blender" || res.executor === "python" || res.executor === "unreal") {
          setExecutor(res.executor);
        }
        if (res.inputs) setInputs(res.inputs);
        if (res.outputs) setOutputs(res.outputs);

        // Auto select the first input pin for configuration
        if (res.inputs && res.inputs.length > 0) {
          setSelectedPin({ pin: res.inputs[0], direction: "in", index: 0 });
        }

        toast.success(
          `Detected ${res.inputs?.length || 0} inputs and ${res.outputs?.length || 0} outputs!`
        );
      }
    } catch {
      // Error handled in hook
    } finally {
      setIsDetecting(false);
    }
  };

  const handleAddInput = () => {
    const newPin: PinDefinition = {
      id: `param_${inputs.length + 1}`,
      label: `Param ${inputs.length + 1}`,
      primitiveType: PinPrimitiveType.NUMBER_0,
      cardinality: PinCardinality.NUMBER_0,
      isRequired: true,
      defaultValue: null,
    };
    const newInputs = [...inputs, newPin];
    setInputs(newInputs);
    setSelectedPin({ pin: newPin, direction: "in", index: newInputs.length - 1 });
  };

  const handleDeleteInput = (index: number) => {
    setInputs((prev) => prev.filter((_, i) => i !== index));
    if (selectedPin?.direction === "in" && selectedPin.index === index) {
      setSelectedPin(null);
    }
  };

  const handleAddOutput = () => {
    const newPin: PinDefinition = {
      id: `output_${outputs.length + 1}`,
      label: `Output ${outputs.length + 1}`,
      primitiveType: PinPrimitiveType.NUMBER_0,
      cardinality: PinCardinality.NUMBER_0,
      isRequired: true,
    };
    const newOutputs = [...outputs, newPin];
    setOutputs(newOutputs);
    setSelectedPin({ pin: newPin, direction: "out", index: newOutputs.length - 1 });
  };

  const handleDeleteOutput = (index: number) => {
    setOutputs((prev) => prev.filter((_, i) => i !== index));
    if (selectedPin?.direction === "out" && selectedPin.index === index) {
      setSelectedPin(null);
    }
  };

  const handleUpdateActivePin = (updated: PinDefinition) => {
    if (!selectedPin) return;

    if (selectedPin.direction === "in") {
      setInputs((prev) =>
        prev.map((p, i) => (i === selectedPin.index ? updated : p))
      );
    } else {
      setOutputs((prev) =>
        prev.map((p, i) => (i === selectedPin.index ? updated : p))
      );
    }

    setSelectedPin({ ...selectedPin, pin: updated });
  };

  const handleSubmit = async () => {
    if (!name.trim()) {
      toast.error("Node Identifier is required.");
      return;
    }

    let finalAssetId: string | null = uploadedAssetId;

    if (!finalAssetId && (selectedFile || scriptContent)) {
      try {
        setIsUploading(true);
        const fileToUpload =
          selectedFile ||
          new File([scriptContent], `${name.trim().replace(/\s+/g, "_")}.py`, {
            type: "text/x-python",
          });

        finalAssetId = await uploadAssetFlow(fileToUpload);
        setUploadedAssetId(finalAssetId);
      } catch (err: any) {
        toast.error("Failed to upload script file: " + (err?.message || ""));
        setIsUploading(false);
        return;
      } finally {
        setIsUploading(false);
      }
    }

    if (editNodeId) {
      await updateNode({
        id: editNodeId,
        data: {
          name: name.trim(),
          label: label.trim() || name.trim(),
          executor,
          assetId: finalAssetId || (existingNode as any)?.assetId || null,
          originalFileName: selectedFile?.name || (existingNode as any)?.originalFileName || `${name.trim()}.py`,
          inputs: inputs as any,
          outputs: outputs as any,
        },
      });
    } else {
      await createNode({
        projectId,
        name: name.trim(),
        label: label.trim() || name.trim(),
        executor,
        assetId: finalAssetId || null,
        originalFileName: selectedFile?.name || `${name.trim()}.py`,
        inputs: inputs as any,
        outputs: outputs as any,
      });
    }

    navigate({
      to: "/projects/$projectId/pipeline/nodes",
      params: { projectId },
    });
  };

  return (
    <div className="space-y-6 pb-12">
      {/* Top Breadcrumb & Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b pb-4">
        <div className="space-y-1">
          <Link
            to="/projects/$projectId/pipeline/nodes"
            params={{ projectId }}
            className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground transition mb-1"
          >
            <ArrowLeft className="size-3.5" /> Back to Node Library
          </Link>
          <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
            <Workflow className="size-6 text-primary" />
            {editNodeId ? "Edit Custom Node" : "Create Custom Node"}
          </h1>
          <p className="text-xs text-muted-foreground">
            {editNodeId
              ? `Editing custom node "${name || editNodeId}"`
              : "Define automation scripts and auto-detect Input/Output Pin schemas."}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="sm"
            onPress={() =>
              navigate({
                to: "/projects/$projectId/pipeline/nodes",
                params: { projectId },
              })
            }
          >
            Cancel
          </Button>
          <Button
            type="button"
            size="sm"
            className="gap-2"
            isDisabled={isCreatingNode || isUpdatingNode || isUploading || isLoadingExisting || !name.trim()}
            onPress={handleSubmit}
          >
            {isCreatingNode || isUpdatingNode || isUploading ? (
              <>
                <Loader2 className="size-3.5 animate-spin mr-1.5" />
                {editNodeId ? "Updating Node..." : "Saving Node..."}
              </>
            ) : (
              <>
                <Save className="size-3.5 mr-1.5" />
                {editNodeId ? "Update Custom Node" : "Save Custom Node"}
              </>
            )}
          </Button>
        </div>
      </div>

      {/* Top Section: Meta & Upload */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <NodeMetaForm
          name={name}
          onChangeName={setName}
          label={label}
          onChangeLabel={setLabel}
          executor={executor}
          onChangeExecutor={setExecutor}
        />

        <ScriptUploadBox
          selectedFile={selectedFile}
          onFileSelect={handleFileSelect}
          scriptContent={scriptContent}
          onChangeScriptContent={setScriptContent}
          isDetecting={isDetecting}
          onAutoDetect={() => autoDetectFromScript(scriptContent)}
        />
      </div>

      {/* Bottom Section: Two-pane Master-Detail (Pin Cards + Config Panel) */}
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-bold uppercase tracking-wider text-muted-foreground">
            Schema Pin Architecture
          </h2>
          <span className="text-xs text-muted-foreground">
            Click on any pin below to open its property inspector
          </span>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-5 items-start">
          {/* Left Column: Pins Frames */}
          <div className="lg:col-span-7 space-y-4">
            <PinCardList
              inputs={inputs}
              outputs={outputs}
              selectedPin={selectedPin}
              onSelectPin={setSelectedPin}
              onAddInput={handleAddInput}
              onDeleteInput={handleDeleteInput}
              onAddOutput={handleAddOutput}
              onDeleteOutput={handleDeleteOutput}
            />
          </div>

          {/* Right Column: Pin Inspector Panel */}
          <div className="lg:col-span-5 sticky top-4">
            <PinConfigInspector
              selectedPin={selectedPin}
              onUpdatePin={handleUpdateActivePin}
              onClose={() => setSelectedPin(null)}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
