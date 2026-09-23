import { keepPreviousData } from "@tanstack/react-query";
import { createMutationHook } from "@/lib/query-utils";
import * as ProjectsApi from "@/gen/endpoints/projects/projects";
import type {
    ProjectExecutorConfigDto,
    UpsertProjectExecutorConfigCommand,
} from "@/gen/model";

export type { ProjectExecutorConfigDto, UpsertProjectExecutorConfigCommand };

export const useProjectExecutorConfigs = (projectId: string) => {
    return ProjectsApi.useGetProjectExecutorConfigs(projectId, {
        query: {
            enabled: !!projectId,
            placeholderData: keepPreviousData,
        },
    });
};

export const useUpsertProjectExecutorConfig = (projectId: string) => {
    const hook = createMutationHook(
        ProjectsApi.useUpsertProjectExecutorConfig,
        [ProjectsApi.getGetProjectExecutorConfigsQueryKey(projectId)]
    )();

    return {
        ...hook,
        mutate: (data: UpsertProjectExecutorConfigCommand, options?: any) =>
            hook.mutate({ projectId, data }, options),
        mutateAsync: (data: UpsertProjectExecutorConfigCommand, options?: any) =>
            hook.mutateAsync({ projectId, data }, options),
    };
};

export const useDeleteProjectExecutorConfig = (projectId: string) => {
    const hook = createMutationHook(
        ProjectsApi.useDeleteProjectExecutorConfig,
        [ProjectsApi.getGetProjectExecutorConfigsQueryKey(projectId)]
    )();

    return {
        ...hook,
        mutate: (id: string, options?: any) =>
            hook.mutate({ projectId, id }, options),
        mutateAsync: (id: string, options?: any) =>
            hook.mutateAsync({ projectId, id }, options),
    };
};
