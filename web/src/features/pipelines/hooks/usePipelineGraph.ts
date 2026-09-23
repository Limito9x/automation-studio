import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createMutationHook } from "@/lib/query-utils";
import { customInstance } from "@/lib/api-client";
import * as PipelinesApi from "@/gen/endpoints/pipelines/pipelines";
import type {
  PipelineGraphDto,
  PipelineNodeGraphDto,
  PipelineEdgeGraphDto,
  PipelineInputDto,
  AddPipelineNodeRequest,
  UpdatePipelineNodeRequest,
  AddPipelineEdgeRequest,
  AddPipelineInputRequest,
  UpdatePipelineInputRequest,
  RunPipelineRequest,
  ValidatePipelineQuery,
  ValidatePipelineResponse,
  PipelineExecutionDto,
  NodeExecutionDto,
  PipelineSummaryDto,
  CreatePipelineCommand,
  EdgeKind,
  ExecutionStatus,
} from "@/gen/model";

export interface PipelineVariableDto {
  name: string;
  type: number | string;
  cardinality?: number | string;
  description?: string | null;
  structType?: string | null;
}

export type ExtendedPipelineGraphDto = PipelineGraphDto & {
  triggerType?: number | string;
  triggerWorkspaceId?: string | null;
  variables?: PipelineVariableDto[];
};

export type {
  PipelineGraphDto,
  PipelineNodeGraphDto,
  PipelineEdgeGraphDto,
  PipelineInputDto,
  AddPipelineNodeRequest,
  UpdatePipelineNodeRequest,
  AddPipelineEdgeRequest,
  AddPipelineInputRequest,
  UpdatePipelineInputRequest,
  RunPipelineRequest,
  ValidatePipelineQuery,
  ValidatePipelineResponse,
  PipelineExecutionDto,
  NodeExecutionDto,
  PipelineSummaryDto,
  CreatePipelineCommand,
  EdgeKind,
  ExecutionStatus,
};

// -----------------------------------------------------------------------------
// Queries
// -----------------------------------------------------------------------------

export const usePipelines = (projectId?: string) => {
  return PipelinesApi.useGetPipelines(
    projectId ? { projectId } : undefined,
    {
      query: {
        placeholderData: keepPreviousData,
      },
    }
  );
};

export const usePipelineGraph = (pipelineId?: string) => {
  return PipelinesApi.useGetPipelineGraph(pipelineId || "", {
    query: {
      enabled: !!pipelineId,
      placeholderData: keepPreviousData,
    },
  });
};

export const usePipelineInputSchema = (pipelineId?: string) => {
  return PipelinesApi.useGetPipelineInputSchema(pipelineId || "", {
    query: {
      enabled: !!pipelineId,
      placeholderData: keepPreviousData,
    },
  });
};

export const getPipelineExecutionsQueryKey = (pipelineId: string) => [
  "/api/pipelines",
  pipelineId,
  "executions",
];

export const usePipelineExecutions = (pipelineId?: string) => {
  return useQuery({
    queryKey: getPipelineExecutionsQueryKey(pipelineId || ""),
    queryFn: ({ signal }) =>
      customInstance<PipelineExecutionDto[]>({
        url: `/api/pipelines/${pipelineId}/executions`,
        method: "GET",
        signal,
      }),
    enabled: !!pipelineId,
    placeholderData: keepPreviousData,
  });
};

export const usePipelineExecution = (executionId?: string) => {
  return PipelinesApi.useGetPipelineExecution(executionId || "", {
    query: {
      enabled: !!executionId,
    },
  });
};

export const useNodeExecutions = (executionId?: string) => {
  return PipelinesApi.useGetNodeExecutions(executionId || "", {
    query: {
      enabled: !!executionId,
      refetchInterval: (query) => {
        // Auto-poll every 3s while execution is active
        const data = query.state.data;
        if (!data || data.length === 0) return 3000;
        const allDone = data.every(
          (n) => n.status === 4 || n.status === 5 || (n.status as any) === "Succeeded" || (n.status as any) === "Failed"
        );
        return allDone ? false : 3000;
      },
    },
  });
};

// -----------------------------------------------------------------------------
// Mutations (Wrapped with createMutationHook & Orval Generated APIs)
// -----------------------------------------------------------------------------

export const useCreatePipeline = (projectId?: string) => {
  const queryKey = projectId ? PipelinesApi.getGetPipelinesQueryKey({ projectId }) : ["pipelines"];
  return createMutationHook(PipelinesApi.useCreatePipeline, [queryKey])();
};

export const useAddPipelineNode = (pipelineId?: string) => {
  const queryKey = pipelineId ? PipelinesApi.getGetPipelineGraphQueryKey(pipelineId) : ["pipelines"];
  const mutation = createMutationHook(PipelinesApi.useAddPipelineNode, [queryKey])();
  return {
    ...mutation,
    mutate: (data: AddPipelineNodeRequest, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, data }, options),
    mutateAsync: (data: AddPipelineNodeRequest, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, data }, options),
  };
};

export const useUpdatePipelineNode = (pipelineId?: string) => {
  const queryKey = pipelineId ? PipelinesApi.getGetPipelineGraphQueryKey(pipelineId) : ["pipelines"];
  const mutation = createMutationHook(PipelinesApi.useUpdatePipelineNode, [queryKey])();
  return {
    ...mutation,
    mutate: ({ nodeId, data }: { nodeId: string; data: UpdatePipelineNodeRequest }, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, nodeId, data }, options),
    mutateAsync: ({ nodeId, data }: { nodeId: string; data: UpdatePipelineNodeRequest }, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, nodeId, data }, options),
  };
};

/**
 * Mutation chuyên dùng cho Drag/Drop tọa độ: Lưu ngầm vào DB mà KHÔNG invalidate query,
 * tránh refetch và reset toàn bộ nodes trên Canvas gây giật lag.
 */
export const useUpdateNodePosition = (pipelineId?: string) => {
  const mutation = PipelinesApi.useUpdatePipelineNode();
  return {
    ...mutation,
    mutate: ({ nodeId, data }: { nodeId: string; data: UpdatePipelineNodeRequest }, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, nodeId, data }, options),
    mutateAsync: ({ nodeId, data }: { nodeId: string; data: UpdatePipelineNodeRequest }, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, nodeId, data }, options),
  };
};

export const useDeletePipelineNode = (pipelineId?: string) => {
  const queryKey = pipelineId ? PipelinesApi.getGetPipelineGraphQueryKey(pipelineId) : ["pipelines"];
  const mutation = createMutationHook(PipelinesApi.useDeletePipelineNode, [queryKey])();
  return {
    ...mutation,
    mutate: (nodeId: string, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, nodeId }, options),
    mutateAsync: (nodeId: string, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, nodeId }, options),
  };
};

export const useAddPipelineEdge = (pipelineId?: string) => {
  const queryKey = pipelineId ? PipelinesApi.getGetPipelineGraphQueryKey(pipelineId) : ["pipelines"];
  const mutation = createMutationHook(PipelinesApi.useAddPipelineEdge, [queryKey])();
  return {
    ...mutation,
    mutate: (data: AddPipelineEdgeRequest, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, data }, options),
    mutateAsync: (data: AddPipelineEdgeRequest, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, data }, options),
  };
};

export const useDeletePipelineEdge = (pipelineId?: string) => {
  const queryKey = pipelineId ? PipelinesApi.getGetPipelineGraphQueryKey(pipelineId) : ["pipelines"];
  const mutation = createMutationHook(PipelinesApi.useDeletePipelineEdge, [queryKey])();
  return {
    ...mutation,
    mutate: (edgeId: string, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, edgeId }, options),
    mutateAsync: (edgeId: string, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, edgeId }, options),
  };
};

export const useRunPipeline = (pipelineId?: string) => {
  const queryKeys = pipelineId
    ? [
        PipelinesApi.getGetPipelineGraphQueryKey(pipelineId),
        ["pipelines", pipelineId, "executions"],
      ]
    : [["pipelines"]];
  const mutation = createMutationHook(PipelinesApi.useRunPipeline, queryKeys)();
  return {
    ...mutation,
    mutate: (data: RunPipelineRequest, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, data }, options),
    mutateAsync: (data: RunPipelineRequest, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, data }, options),
  };
};

export const useValidatePipeline = (pipelineId?: string) => {
  const mutation = createMutationHook(PipelinesApi.useValidatePipeline, [])();
  return {
    ...mutation,
    mutate: (data: ValidatePipelineQuery, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, data }, options),
    mutateAsync: (data: ValidatePipelineQuery, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, data }, options),
  };
};

export const useAddPipelineInput = (pipelineId?: string) => {
  const queryKeys = pipelineId
    ? [
        PipelinesApi.getGetPipelineGraphQueryKey(pipelineId),
        PipelinesApi.getGetPipelineInputSchemaQueryKey(pipelineId),
      ]
    : [["pipelines"]];
  const mutation = createMutationHook(PipelinesApi.useAddPipelineInput, queryKeys)();
  return {
    ...mutation,
    mutate: (data: AddPipelineInputRequest, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, data }, options),
    mutateAsync: (data: AddPipelineInputRequest, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, data }, options),
  };
};

export const useUpdatePipelineInput = (pipelineId?: string) => {
  const queryKeys = pipelineId
    ? [
        PipelinesApi.getGetPipelineGraphQueryKey(pipelineId),
        PipelinesApi.getGetPipelineInputSchemaQueryKey(pipelineId),
      ]
    : [["pipelines"]];
  const mutation = createMutationHook(PipelinesApi.useUpdatePipelineInput, queryKeys)();
  return {
    ...mutation,
    mutate: ({ inputId, data }: { inputId: string; data: UpdatePipelineInputRequest }, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, inputId, data }, options),
    mutateAsync: ({ inputId, data }: { inputId: string; data: UpdatePipelineInputRequest }, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, inputId, data }, options),
  };
};

export const useDeletePipelineInput = (pipelineId?: string) => {
  const queryKeys = pipelineId
    ? [
        PipelinesApi.getGetPipelineGraphQueryKey(pipelineId),
        PipelinesApi.getGetPipelineInputSchemaQueryKey(pipelineId),
      ]
    : [["pipelines"]];
  const mutation = createMutationHook(PipelinesApi.useDeletePipelineInput, queryKeys)();
  return {
    ...mutation,
    mutate: (inputId: string, options?: any) =>
      mutation.mutate({ pipelineId: pipelineId!, inputId }, options),
    mutateAsync: (inputId: string, options?: any) =>
      mutation.mutateAsync({ pipelineId: pipelineId!, inputId }, options),
  };
};

export const useUpdatePipelineVariables = (pipelineId?: string) => {
  const queryClient = useQueryClient();
  const queryKey = pipelineId ? PipelinesApi.getGetPipelineGraphQueryKey(pipelineId) : ["pipelines"];
  return useMutation({
    mutationFn: (variables: PipelineVariableDto[]) =>
      customInstance<PipelineVariableDto[]>({
        url: `/api/pipelines/${pipelineId}/variables`,
        method: "PUT",
        data: { pipelineId, variables },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });
};

export const useUpdatePipelineTrigger = (pipelineId?: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { triggerType: number; triggerWorkspaceId?: string | null; triggerConfig?: unknown }) =>
      customInstance<any>({
        url: `/api/pipelines/${pipelineId}/trigger`,
        method: "PUT",
        data,
      }),
    onSuccess: () => {
      if (pipelineId) {
        queryClient.invalidateQueries({
          queryKey: PipelinesApi.getGetPipelineGraphQueryKey(pipelineId),
        });
        queryClient.invalidateQueries({
          queryKey: PipelinesApi.getGetPipelineInputSchemaQueryKey(pipelineId),
        });
      }
      queryClient.invalidateQueries({ queryKey: ["pipelines"] });
    },
  });
};


