import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import { CreateWorkspaceForm } from "../components/CreateWorkspaceForm";
import { useUpdateWorkspace } from "../hooks/useWorkspaces";
import type { DialogProps } from "@/lib/dialog-registry";

export interface UpdateWorkspaceDialogProps {
  id: string;
  name: string;
  platformIds?: string[];
}

export function UpdateWorkspaceDialog({ open, onOpenChange, data }: DialogProps<UpdateWorkspaceDialogProps>) {
  const updateWorkspace = useUpdateWorkspace();

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Edit Workspace"
      formId="workspace-form"
      isPending={updateWorkspace.isPending}
      size="md"
    >
      <CreateWorkspaceForm
        defaultValues={{ name: data?.name, platformIds: data?.platformIds }}
        onSubmit={(values) => {
          if (!data) return;
          updateWorkspace.mutate(
            { id: data.id, data: { name: values.name, platformIds: values.platformIds } },
            {
              onSuccess: () => onOpenChange(false),
            }
          );
        }}
      />
    </BaseFormDialog>
  );
}
