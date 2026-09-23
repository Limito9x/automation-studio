import { toast } from "sonner";
import { ConfirmDialog } from "@/components/custom-ui/overlays/dialog/ConfirmDialog";
import { useDeleteExtension } from "../hooks/usePlatforms";
import type { DialogProps } from "@/lib/dialog-registry";

export function DeleteExtensionDialog({ open, onOpenChange, data }: DialogProps<{ id: string }>) {
    const deleteExtension = useDeleteExtension();

    const handleDelete = () => {
        if (!data?.id) return;
        deleteExtension.mutate(
            { id: data.id },
            {
                onSuccess: () => {
                    toast.success("Extension deleted successfully");
                    onOpenChange(false);
                },
                onError: (error: any) => {
                    toast.error(error?.message || "An error occurred");
                }
            }
        );
    };

    return (
        <ConfirmDialog
            open={open}
            onOpenChange={onOpenChange}
            title="Delete Extension"
            description="Are you sure you want to delete this extension? This action cannot be undone."
            confirmText="Delete"
            cancelText="Cancel"
            variant="destructive"
            isLoading={deleteExtension.isPending}
            onConfirm={handleDelete}
        />
    );
}
