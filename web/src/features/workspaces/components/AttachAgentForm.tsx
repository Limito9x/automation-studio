import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { useWatch } from "react-hook-form";
import { FormInput, FormSelect } from "@/components/form-controls";
import { attachAgentSchema, type AttachAgentInput, type AttachAgentOutput } from "../schemas/workspaceSchema";
import { useGetAgents } from "@/gen/endpoints/agents/agents";
import { FolderBrowser } from "@/components/custom-ui/file-tree";
import { FolderTree } from "lucide-react";

interface AttachAgentFormProps {
  workspaceId?: string;
  onSubmit: (values: AttachAgentOutput) => void;
}

export function AttachAgentForm({ workspaceId, onSubmit }: AttachAgentFormProps) {
  // Query available agents
  const { data: agents = [] } = useGetAgents();

  const agentOptions = (agents as any[]).map((a) => ({
    label: `${a.name || a.id} (${a.machineKey || "Unknown Machine"})`,
    value: a.id,
  }));

  const form = useForm<AttachAgentInput, any, AttachAgentOutput>({
    resolver: zodResolver(attachAgentSchema),
    defaultValues: {
      agentId: "",
      rootPath: "",
    },
  });

  const selectedAgentId = useWatch({ control: form.control, name: "agentId" });
  const currentRootPath = useWatch({ control: form.control, name: "rootPath" });

  return (
    <Form form={form} formId="attach-agent-form" onSubmit={onSubmit}>
      <FormGrid cols={1} className="gap-4">
        <FormSelect
          control={form.control}
          label="Select Agent"
          name="agentId"
          placeholder="Choose an agent..."
          options={agentOptions}
        />

        {/* Chỉ hiển thị Root Path và Folder Browser khi đã chọn Agent */}
        {selectedAgentId && workspaceId && (
          <>
            <FormInput
              control={form.control}
              label="Root Path"
              name="rootPath"
              type="text"
              readOnly
              placeholder="Choose directory below to auto-fill Root Path..."
              className="bg-muted/50 font-mono text-xs cursor-default"
            />

            {/* Folder Browser */}
            <div className="space-y-2 pt-2 border-t border-border/40">
              <div className="flex items-center justify-between">
                <label className="text-xs font-medium text-foreground flex items-center gap-1.5">
                  <FolderTree className="size-3.5 text-primary" />
                  <span>Browse & Select Directory</span>
                </label>
                <span className="text-[11px] text-muted-foreground">
                  Click to select / Double-click or press Open to open
                </span>
              </div>

              <FolderBrowser
                agentId={selectedAgentId}
                selectedPath={currentRootPath}
                height={280}
                onSelectPath={(path) => {
                  form.setValue("rootPath", path, { shouldValidate: true });
                }}
              />
            </div>
          </>
        )}
      </FormGrid>
    </Form>
  );
}
