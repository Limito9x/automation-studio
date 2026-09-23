import { ConfirmDialog } from "@/components/custom-ui/overlays/dialog/ConfirmDialog";
import { useDeleteWorkspace } from "../hooks/useWorkspaces";
import type { DialogProps } from "@/lib/dialog-registry";

export function DeleteWorkspaceDialog({ open, onOpenChange, data }: DialogProps<{ id: string; name: string }>) {
  const deleteWorkspace = useDeleteWorkspace();

  return (
    <ConfirmDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Delete Workspace"
      description={`Are you sure you want to delete workspace "${data?.name || ""}"? This action cannot be undone.`}
      confirmText="Delete"
      variant="destructive"
      isLoading={deleteWorkspace.isPending}
      onConfirm={() => {
        if (!data) return;
        deleteWorkspace.mutate(
          { id: data.id },
          {
            onSuccess: () => onOpenChange(false),
          }
        );
      }}
    />
  );
}
