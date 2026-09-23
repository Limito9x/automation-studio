import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { createMutationHook } from "@/lib/query-utils";
import * as WorkspacesApi from "@/gen/endpoints/workspaces/workspaces";
import * as ResourcesApi from "@/gen/endpoints/resources/resources";
import type { GetWorkspaceResourcesParams } from "@/gen/model";

export type { GetWorkspaceResourcesParams };

export const useWorkspaceResources = (
  workspaceId: string,
  params: GetWorkspaceResourcesParams,
  options?: { enabled?: boolean }
) => {
  return WorkspacesApi.useGetWorkspaceResources(workspaceId, params, {
    query: {
      enabled: Boolean(workspaceId) && (options?.enabled ?? true),
      placeholderData: keepPreviousData,
    },
  });
};

export const useGetResourceById = (
  id: string,
  options?: { enabled?: boolean }
) => {
  return ResourcesApi.useGetResourceById(id, {
    query: {
      enabled: Boolean(id) && (options?.enabled ?? true),
    },
  });
};

export const useGetResourcesByContent = (
  contentId: string,
  options?: { enabled?: boolean }
) => {
  return ResourcesApi.useGetResourcesByContent(contentId, {
    query: {
      enabled: Boolean(contentId) && (options?.enabled ?? true),
      placeholderData: keepPreviousData,
    },
  });
};

export const useAssignResourcesContent = (workspaceId?: string) => {
  const queryKeys = workspaceId
    ? [WorkspacesApi.getGetWorkspaceResourcesQueryKey(workspaceId)]
    : [["resources"]];
  return createMutationHook(ResourcesApi.useAssignResourcesContent, queryKeys)();
};

export const useDeleteResource = (workspaceId?: string) => {
  const queryKeys = workspaceId
    ? [WorkspacesApi.getGetWorkspaceResourcesQueryKey(workspaceId)]
    : [["resources"]];
  return createMutationHook(ResourcesApi.useDeleteResource, queryKeys)();
};

export const useAvailableAgents = (
  workspaceId: string,
  resourceIds: string[],
  options?: { enabled?: boolean }
) => {
  return useQuery({
    queryKey: ["resources", "available-agents", workspaceId, resourceIds],
    queryFn: () =>
      ResourcesApi.getAvailableAgents({
        workspaceId,
        resourceIds,
      }),
    enabled:
      Boolean(workspaceId) &&
      resourceIds.length > 0 &&
      (options?.enabled ?? true),
  });
};
