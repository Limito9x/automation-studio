import { toast } from "sonner";
import { ConfirmDialog } from "@/components/custom-ui/overlays/dialog/ConfirmDialog";
import { useDeletePlatform } from "../hooks/usePlatforms";
import type { DialogProps } from "@/lib/dialog-registry";

export function DeletePlatformDialog({ open, onOpenChange, data }: DialogProps<{ id: string }>) {
    const deletePlatform = useDeletePlatform();

    const handleDelete = () => {
        if (!data?.id) return;
        deletePlatform.mutate(
            { id: data.id },
            {
                onSuccess: () => {
                    toast.success("Platform deleted successfully");
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
            title="Delete Platform"
            description="Are you sure you want to delete this platform? This action cannot be undone."
            confirmText="Delete"
            cancelText="Cancel"
            variant="destructive"
            isLoading={deletePlatform.isPending}
            onConfirm={handleDelete}
        />
    );
}
