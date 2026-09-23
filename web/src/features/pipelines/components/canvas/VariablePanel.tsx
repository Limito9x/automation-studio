import { useState } from "react";
import {
  Variable,
  Plus,
  Trash2,
  ChevronLeft,
  ChevronRight,
  Database,
  GripVertical,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import type { PipelineVariableDto } from "../../hooks/usePipelineGraph";
import { useUpdatePipelineVariables } from "../../hooks/usePipelineGraph";
import { usePipelineFormScope } from "../../form-scope/PipelineFormScope";
import { useGetProjectStructs } from "@/features/structs/hooks/useStructs";
import { toast } from "sonner";

interface VariablePanelProps {
  pipelineId: string;
  variables: PipelineVariableDto[];
  onSpawnNode?: (toolKey: "GetVariable" | "SetVariable", varName: string) => void;
  isOpen: boolean;
  onToggle: () => void;
}

const TYPE_OPTIONS = [
  { value: "0", label: "String", color: "text-sky-500 bg-sky-500/10 border-sky-500/30" },
  { value: "1", label: "Number", color: "text-violet-500 bg-violet-500/10 border-violet-500/30" },
  { value: "2", label: "Boolean", color: "text-amber-500 bg-amber-500/10 border-amber-500/30" },
  { value: "3", label: "Path", color: "text-orange-500 bg-orange-500/10 border-orange-500/30" },
  { value: "4", label: "EntityRef / Struct", color: "text-emerald-500 bg-emerald-500/10 border-emerald-500/30" },
  { value: "5", label: "Asset", color: "text-pink-500 bg-pink-500/10 border-pink-500/30" },
];

const CARDINALITY_OPTIONS = [
  { value: "0", label: "Single" },
  { value: "1", label: "Array []" },
  { value: "2", label: "Map {}" },
];

export function VariablePanel({
  pipelineId,
  variables = [],
  onSpawnNode,
  isOpen,
  onToggle,
}: VariablePanelProps) {
  const { projectId = "" } = usePipelineFormScope();
  const { data: projectStructs = [] } = useGetProjectStructs(projectId);

  const [isCreating, setIsCreating] = useState(false);
  const [name, setName] = useState("");
  const [type, setType] = useState("0");
  const [cardinality, setCardinality] = useState("0");
  const [structType, setStructType] = useState("Resource");
  const [description, setDescription] = useState("");

  const updateMutation = useUpdatePipelineVariables(pipelineId);

  const handleCreate = async () => {
    const trimmed = name.trim();
    if (!trimmed) {
      toast.error("Variable name is required.");
      return;
    }

    if (variables.some((v) => v.name.toLowerCase() === trimmed.toLowerCase())) {
      toast.error(`Variable '${trimmed}' already exists.`);
      return;
    }

    const newVar: PipelineVariableDto = {
      name: trimmed,
      type: Number(type),
      cardinality: Number(cardinality),
      description: description.trim() || null,
      structType: type === "4" ? structType : undefined,
    };

    const nextVars = [...variables, newVar];
    try {
      await updateMutation.mutateAsync(nextVars);
      toast.success(`Declared variable '${trimmed}'`);
      setName("");
      setDescription("");
      setIsCreating(false);
    } catch (err: any) {
      toast.error(err?.message || "Failed to add variable");
    }
  };

  const handleDelete = async (varName: string) => {
    const nextVars = variables.filter((v) => v.name !== varName);
    try {
      await updateMutation.mutateAsync(nextVars);
      toast.success(`Removed variable '${varName}'`);
    } catch (err: any) {
      toast.error(err?.message || "Failed to remove variable");
    }
  };

  const getTypeStyle = (t: number | string, c?: number | string, sType?: string | null) => {
    const tStr = String(t);
    const opt = TYPE_OPTIONS.find((o) => o.value === tStr) || TYPE_OPTIONS[0];
    const isMap = c === 2 || c === "2" || c === "Map";
    const isArr = c === 1 || c === "1" || c === "Array";
    const label = tStr === "4" && sType ? sType : opt.label;

    return {
      label: isMap ? `Map<${label}>` : isArr ? `${label}[]` : label,
      color: opt.color,
    };
  };

  return (
    <aside
      className={cn(
        "absolute left-3 top-16 bottom-3 z-20 flex flex-col transition-all duration-300 ease-in-out",
        isOpen ? "w-80" : "w-10"
      )}
    >
      <div className="flex h-full flex-col rounded-xl border border-border/80 bg-background/95 backdrop-blur-md shadow-2xl overflow-hidden">
        {/* Panel Header */}
        <div className="flex h-11 items-center justify-between border-b border-border/60 px-3 bg-muted/30">
          <div className="flex items-center gap-2 overflow-hidden">
            <Variable className="h-4 w-4 shrink-0 text-cyan-500" />
            {isOpen && (
              <div className="flex items-center gap-1.5 font-semibold text-xs text-foreground truncate">
                <span>Variables</span>
                <Badge variant="secondary" className="px-1.5 py-0 h-4 text-[10px] font-mono">
                  {variables.length}
                </Badge>
              </div>
            )}
          </div>

          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7 text-muted-foreground hover:text-foreground"
            onPress={onToggle}
          >
            {isOpen ? <ChevronLeft className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
          </Button>
        </div>

        {isOpen && (
          <div className="flex flex-1 flex-col overflow-hidden p-3 gap-3">
            {/* Action Bar */}
            <div className="flex items-center justify-between">
              <span className="text-[11px] font-medium text-muted-foreground">
                Execution Blackboard
              </span>
              {!isCreating && (
                <Button
                  size="sm"
                  variant="outline"
                  className="h-7 text-xs gap-1 border-primary/40 bg-primary/5 text-primary hover:bg-primary/10"
                  onPress={() => setIsCreating(true)}
                >
                  <Plus className="h-3.5 w-3.5" />
                  <span>New Variable</span>
                </Button>
              )}
            </div>

            {/* Inline Creation Form */}
            {isCreating && (
              <div className="rounded-lg border border-primary/30 bg-primary/5 p-3 space-y-2.5 shadow-sm">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-primary">New Variable</span>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-5 px-1.5 text-[10px] text-muted-foreground hover:text-foreground"
                    onPress={() => setIsCreating(false)}
                  >
                    Cancel
                  </Button>
                </div>

                <div className="space-y-1.5">
                  <label className="text-[10px] font-medium text-muted-foreground block">
                    Name
                  </label>
                  <Input
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="e.g. ExportMap, OutputDir"
                    className="h-7 text-xs font-mono"
                    autoFocus
                  />
                </div>

                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <label className="text-[10px] font-medium text-muted-foreground block">
                      Type
                    </label>
                    <Select
                      selectedKey={type}
                      onSelectionChange={(key) => setType(String(key))}
                      className="w-full"
                    >
                      <SelectTrigger className="h-7 w-full text-xs">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {TYPE_OPTIONS.map((t) => (
                          <SelectItem key={t.value} id={t.value}>
                            {t.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-1">
                    <label className="text-[10px] font-medium text-muted-foreground block">
                      Structure
                    </label>
                    <Select
                      selectedKey={cardinality}
                      onSelectionChange={(key) => setCardinality(String(key))}
                      className="w-full"
                    >
                      <SelectTrigger className="h-7 w-full text-xs">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {CARDINALITY_OPTIONS.map((c) => (
                          <SelectItem key={c.value} id={c.value}>
                            {c.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                {type === "4" && (
                  <div className="space-y-1">
                    <label className="text-[10px] font-medium text-muted-foreground block">
                      Target Struct
                    </label>
                    <Select
                      selectedKey={structType}
                      onSelectionChange={(key) => setStructType(String(key))}
                      className="w-full"
                    >
                      <SelectTrigger className="h-7 w-full text-xs">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem id="Resource">Resource</SelectItem>
                        <SelectItem id="Workspace">Workspace</SelectItem>
                        <SelectItem id="Inspection">Inspection</SelectItem>
                        <SelectItem id="TaggedAsset">Tagged Asset</SelectItem>
                        {projectStructs.map((s) => (
                          <SelectItem key={s.id} id={s.name}>
                            {s.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                )}

                <div className="space-y-1">
                  <label className="text-[10px] font-medium text-muted-foreground block">
                    Description (optional)
                  </label>
                  <Input
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    placeholder="Brief purpose of this variable..."
                    className="h-7 text-xs"
                  />
                </div>

                <Button
                  size="sm"
                  className="w-full h-7 text-xs font-medium"
                  onPress={handleCreate}
                  isDisabled={updateMutation.isPending || !name.trim()}
                >
                  {updateMutation.isPending ? "Saving..." : "Declare Variable"}
                </Button>
              </div>
            )}

            {/* Variable List */}
            <ScrollArea className="flex-1 -mx-1 px-1">
              {variables.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-10 px-4 text-center text-muted-foreground">
                  <Database className="h-8 w-8 mb-2 opacity-30 text-cyan-500" />
                  <p className="text-xs font-medium">No variables declared</p>
                  <p className="text-[10px] text-muted-foreground/70 mt-1 max-w-[200px]">
                    Declare pipeline variables here to persist and share data across nodes and loops.
                  </p>
                </div>
              ) : (
                <div className="space-y-1.5">
                  {variables.map((v) => {
                    const style = getTypeStyle(v.type, v.cardinality, v.structType);
                    return (
                      <div
                        key={v.name}
                        draggable
                        onDragStart={(e) => {
                          e.dataTransfer.setData("application/pipeline-variable", JSON.stringify(v));
                          e.dataTransfer.effectAllowed = "move";
                        }}
                        onDoubleClick={() => onSpawnNode?.("GetVariable", v.name)}
                        className="group relative flex items-center justify-between gap-2 rounded-lg border border-border/70 bg-card px-2.5 py-2 hover:border-cyan-500/50 hover:bg-cyan-500/5 cursor-grab active:cursor-grabbing transition-all shadow-sm select-none"
                        title="Drag to Canvas (Hold Ctrl for Get, Alt for Set)"
                      >
                        <div className="flex items-center gap-2 min-w-0">
                          <GripVertical className="h-3.5 w-3.5 text-muted-foreground/40 group-hover:text-cyan-500 shrink-0 transition-colors" />
                          <div className="flex flex-col min-w-0">
                            <span className="font-mono text-xs font-semibold text-foreground truncate group-hover:text-cyan-600 dark:group-hover:text-cyan-400">
                              {v.name}
                            </span>
                            {v.description && (
                              <span className="text-[10px] text-muted-foreground truncate max-w-[130px]">
                                {v.description}
                              </span>
                            )}
                          </div>
                        </div>

                        <div className="flex items-center gap-1.5 shrink-0">
                          <Badge
                            variant="outline"
                            className={cn("text-[9px] font-mono px-1.5 h-4 font-medium", style.color)}
                          >
                            {style.label}
                          </Badge>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-5 w-5 opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive transition-all"
                            onPress={() => handleDelete(v.name)}
                          >
                            <Trash2 className="h-3 w-3" />
                          </Button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </ScrollArea>

            {variables.length > 0 && (
              <div className="pt-2 border-t border-border/50 text-[10px] text-muted-foreground text-center">
                <span>Drag to Canvas to place</span>
                <span className="text-muted-foreground/60 block text-[9px] font-mono mt-0.5">
                  Hold Ctrl for Get, Alt for Set
                </span>
              </div>
            )}
          </div>
        )}
      </div>
    </aside>
  );
}
