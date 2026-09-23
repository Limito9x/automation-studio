import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import type { DialogProps } from "@/lib/dialog-registry";
import { useTranslation } from "react-i18next";
import { UpsertProjectExecutorConfigForm } from "../components/UpsertProjectExecutorConfigForm";
import {
    useUpsertProjectExecutorConfig,
    type ProjectExecutorConfigDto,
} from "../hooks/useProjectExecutorConfigs";

export interface UpsertProjectExecutorConfigDialogProps {
    projectId: string;
    config?: ProjectExecutorConfigDto;
}

export function UpsertProjectExecutorConfigDialog({
    open,
    onOpenChange,
    data,
}: DialogProps<UpsertProjectExecutorConfigDialogProps>) {
    const { t } = useTranslation("projects");
    const projectId = data?.projectId || "";
    const config = data?.config;

    const upsertConfig = useUpsertProjectExecutorConfig(projectId);
    const isPending = upsertConfig.isPending;

    // Extract default values from JSON settings if editing
    let settingsObj: Record<string, any> = {};
    if (config?.settings) {
        try {
            settingsObj = typeof config.settings === "string"
                ? JSON.parse(config.settings)
                : config.settings;
        } catch {
            settingsObj = {};
        }
    }

    const defaultValues = config
        ? {
            agentId: config.agentId,
            executorKey: config.executorKey,
            fullPathProject: settingsObj.fullPathProject || settingsObj.project_path || "",
            engineVersion: settingsObj.engineVersion || "",
        }
        : undefined;

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={
                config
                    ? t("actions.editExecutorConfig", { defaultValue: "Edit Executor Configuration" })
                    : t("actions.addExecutorConfig", { defaultValue: "Configure Agent Executor" })
            }
            formId="upsert-project-executor-config-form"
            isPending={isPending}
            size="md"
        >
            <UpsertProjectExecutorConfigForm
                defaultValues={defaultValues}
                onSubmit={(values) => {
                    upsertConfig.mutate(
                        {
                            agentId: values.agentId,
                            executorKey: values.executorKey,
                            settings: values.settings,
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
