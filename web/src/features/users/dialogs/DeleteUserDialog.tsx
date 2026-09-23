import { toast } from "sonner";
import { ConfirmDialog } from "@/components/custom-ui/overlays/dialog/ConfirmDialog";
import { useDeleteUser } from "../hooks/useUsers";
import type { DialogProps } from "@/lib/dialog-registry";

export function DeleteUserDialog({ open, onOpenChange, data }: DialogProps<{ id: string }>) {
    const deleteUser = useDeleteUser();

    const handleDelete = () => {
        if (!data?.id) return;
        deleteUser.mutate(
            { id: data.id },
            {
                onSuccess: () => {
                    toast.success("Xoá người dùng thành công");
                    onOpenChange(false);
                },
                onError: (error: any) => {
                    toast.error(error.message || "Có lỗi xảy ra");
                }
            }
        );
    };

    return (
        <ConfirmDialog
            open={open}
            onOpenChange={onOpenChange}
            title="Xác nhận xoá người dùng"
            description="Bạn có chắc chắn muốn xoá người dùng này không? Hành động này không thể hoàn tác."
            confirmText="Xoá"
            cancelText="Huỷ"
            variant="destructive"
            isLoading={deleteUser.isPending}
            onConfirm={handleDelete}
        />
    );
}
