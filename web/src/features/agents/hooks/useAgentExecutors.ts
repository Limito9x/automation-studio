import { useMutation, useQueryClient } from "@tanstack/react-query";
import { scanExecutors, configureAgentExecutor, getGetAgentsQueryKey } from "@/gen/endpoints/agents/agents";
import type { ConfigureAgentExecutorCommand, ScanExecutorsCommand, ExecutorCandidateDto } from "@/gen/model";
import { toast } from "sonner";
import { useTranslation } from "react-i18next";

export function useAgentExecutors(agentId: string) {
    const queryClient = useQueryClient();
    const { t } = useTranslation();

    const scanMutation = useMutation({
        mutationFn: async (data: ScanExecutorsCommand) => {
            const res = await scanExecutors(agentId, data);
            return res as unknown as ExecutorCandidateDto[];
        },
        onError: (err: any) => {
            const errorMsg = err?.response?.data?.message || err?.message || "Failed to scan executables on agent";
            toast.error(t("agents.scanFailed", { defaultValue: errorMsg }));
        }
    });

    const configureMutation = useMutation({
        mutationFn: async (data: ConfigureAgentExecutorCommand) => {
            const res = await configureAgentExecutor(agentId, data);
            return res;
        },
        onSuccess: () => {
            toast.success(t("agents.configureSuccess", { defaultValue: "Executor configuration saved successfully" }));
            queryClient.invalidateQueries({ queryKey: getGetAgentsQueryKey() });
        },
        onError: (err: any) => {
            const errorMsg = err?.response?.data?.message || err?.message || "Failed to save executor config";
            toast.error(t("agents.configureFailed", { defaultValue: errorMsg }));
        }
    });

    return {
        scanExecutors: scanMutation.mutateAsync,
        isScanning: scanMutation.isPending,
        scanCandidates: scanMutation.data,
        resetScan: scanMutation.reset,

        configureExecutor: configureMutation.mutateAsync,
        isConfiguring: configureMutation.isPending,
    };
}
