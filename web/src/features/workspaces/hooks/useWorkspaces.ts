import { keepPreviousData } from "@tanstack/react-query";
import { createMutationHook } from "@/lib/query-utils";
import * as WorkspacesApi from "@/gen/endpoints/workspaces/workspaces";
import type {
  WorkspaceDto,
  WorkspaceDetailDto,
  WorkspaceAgentDto,
  CreateWorkspaceCommand,
  UpdateWorkspaceRequest,
  AttachAgentToWorkspaceCommand,
  DirectoryNodeDto,
  DiffResult,
  ResourceDiffItem,
  SyncLocalChangesCommand,
  SyncLocalChangesResult,
} from "@/gen/model";

export type {
  WorkspaceDto,
  WorkspaceDetailDto,
  WorkspaceAgentDto,
  CreateWorkspaceCommand,
  UpdateWorkspaceRequest,
  AttachAgentToWorkspaceCommand,
  DirectoryNodeDto,
  DiffResult,
  ResourceDiffItem,
  SyncLocalChangesCommand,
  SyncLocalChangesResult,
};

export const useWorkspaces = (projectId: string) => {
  return WorkspacesApi.useGetWorkspaces(projectId, {
    query: {
      enabled: !!projectId,
      placeholderData: keepPreviousData,
    },
  });
};

export const useWorkspaceDetail = (workspaceId: string) => {
  return WorkspacesApi.useGetWorkspaceById(workspaceId, {
    query: {
      enabled: !!workspaceId,
    },
  });
};

export const useCreateWorkspace = (projectId?: string) => {
  const queryKey = projectId ? WorkspacesApi.getGetWorkspacesQueryKey(projectId) : ["workspaces"];
  return createMutationHook(WorkspacesApi.useCreateWorkspace, [queryKey])();
};

export const useUpdateWorkspace = (projectId?: string) => {
  const queryKey = projectId ? WorkspacesApi.getGetWorkspacesQueryKey(projectId) : ["workspaces"];
  return createMutationHook(WorkspacesApi.useUpdateWorkspace, [queryKey])();
};

export const useDeleteWorkspace = (projectId?: string) => {
  const queryKey = projectId ? WorkspacesApi.getGetWorkspacesQueryKey(projectId) : ["workspaces"];
  return createMutationHook(WorkspacesApi.useDeleteWorkspace, [queryKey])();
};

export const useAttachAgentToWorkspace = (workspaceId?: string) => {
  const queryKey = workspaceId ? WorkspacesApi.getGetWorkspaceByIdQueryKey(workspaceId) : ["workspaces"];
  return createMutationHook(WorkspacesApi.useAttachAgentToWorkspace, [queryKey])();
};


export const useCompareWorkspace = () => {
  return createMutationHook(WorkspacesApi.useSCompareWorkspaceResource, [])();
};

export const useSyncLocalChanges = (workspaceId?: string) => {
  const queryKeys = workspaceId
    ? [
        WorkspacesApi.getGetWorkspaceByIdQueryKey(workspaceId),
        ["workspaces", workspaceId, "resources"],
      ]
    : [["workspaces"]];
  return createMutationHook(WorkspacesApi.useSyncLocalChanges, queryKeys)();
};
