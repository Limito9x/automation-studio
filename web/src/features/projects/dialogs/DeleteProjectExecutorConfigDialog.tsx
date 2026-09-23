import { toast } from "sonner";
import { ConfirmDialog } from "@/components/custom-ui/overlays/dialog/ConfirmDialog";
import type { DialogProps } from "@/lib/dialog-registry";
import { useTranslation } from "react-i18next";
import { useDeleteProjectExecutorConfig } from "../hooks/useProjectExecutorConfigs";

export interface DeleteProjectExecutorConfigDialogProps {
    projectId: string;
    id: string;
    executorKey?: string;
}

export function DeleteProjectExecutorConfigDialog({
    open,
    onOpenChange,
    data,
}: DialogProps<DeleteProjectExecutorConfigDialogProps>) {
    const { t } = useTranslation("projects");
    const projectId = data?.projectId || "";
    const deleteConfig = useDeleteProjectExecutorConfig(projectId);

    const handleDelete = () => {
        if (!data?.id) return;
        deleteConfig.mutate(data.id, {
            onSuccess: () => {
                toast.success(t("actions.deleteSuccess", { defaultValue: "Deleted successfully" }));
                onOpenChange(false);
            },
        });
    };

    return (
        <ConfirmDialog
            open={open}
            onOpenChange={onOpenChange}
            title={t("actions.deleteExecutorConfig", { defaultValue: "Delete Executor Configuration" })}
            description={t("actions.deleteExecutorConfigConfirm", {
                defaultValue: "Are you sure you want to remove this executor configuration from this agent?",
            })}
            confirmText={t("actions.delete", { defaultValue: "Delete" })}
            cancelText={t("cancel", { ns: "common", defaultValue: "Cancel" })}
            variant="destructive"
            isLoading={deleteConfig.isPending}
            onConfirm={handleDelete}
        />
    );
}
