import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import { AttachAgentForm } from "../components/AttachAgentForm";
import { useAttachAgentToWorkspace } from "../hooks/useWorkspaces";
import type { DialogProps } from "@/lib/dialog-registry";

export function AttachAgentDialog({ open, onOpenChange, data }: DialogProps<{ workspaceId: string }>) {
  const attachAgent = useAttachAgentToWorkspace(data?.workspaceId);

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Add Agent to Workspace"
      formId="attach-agent-form"
      isPending={attachAgent.isPending}
      size="lg"
    >
      <AttachAgentForm
        workspaceId={data?.workspaceId}
        onSubmit={(values) => {
          if (!data) return;
          attachAgent.mutate(
            {
              workspaceId: data.workspaceId,
              data: {
                agentId: values.agentId,
                rootPath: values.rootPath,
              },
            },
            {
              onSuccess: () => onOpenChange(false),
            }
          );
        }}
      />
    </BaseFormDialog>
  );
}
