import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import { CreateWorkspaceForm } from "../components/CreateWorkspaceForm";
import { useCreateWorkspace } from "../hooks/useWorkspaces";
import type { DialogProps } from "@/lib/dialog-registry";

export function CreateWorkspaceDialog({ open, onOpenChange, data }: DialogProps<{ projectId: string }>) {
  const createWorkspace = useCreateWorkspace(data?.projectId);

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Create Workspace"
      formId="workspace-form"
      isPending={createWorkspace.isPending}
      size="md"
    >
      <CreateWorkspaceForm
        onSubmit={(values) => {
          if (!data) return;
          createWorkspace.mutate(
            { data: { projectId: data.projectId, name: values.name, platformIds: values.platformIds } },
            {
              onSuccess: () => onOpenChange(false),
            }
          );
        }}
      />
    </BaseFormDialog>
  );
}
