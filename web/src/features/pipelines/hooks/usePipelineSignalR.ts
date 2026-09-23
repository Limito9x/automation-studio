import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { getGetPipelineExecutionsQueryKey, getGetPipelineExecutionQueryKey } from "@/gen/endpoints/pipelines/pipelines";

export function usePipelineSignalR(
  pipelineId?: string,
  callbacks?: {
    onExecutionStarted?: (executionId: string) => void;
    onNodeExecutionUpdated?: (executionId: string, nodeId: string, status?: string) => void;
    onExecutionFinished?: (executionId: string) => void;
  }
) {
  const queryClient = useQueryClient();
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!pipelineId) return;

    const baseUrl = (import.meta.env.VITE_API_URL as string) || "";
    const hubUrl = `${baseUrl.replace(/\/+$/, "")}/hubs/pipeline-executions`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on("PipelineExecutionStarted", (data: { executionId: string; pipelineId: string; status: number }) => {
      queryClient.invalidateQueries({
        queryKey: getGetPipelineExecutionsQueryKey(data.pipelineId),
      });
      queryClient.invalidateQueries({
        queryKey: getGetPipelineExecutionQueryKey(data.executionId),
      });
      callbacks?.onExecutionStarted?.(data.executionId);
    });

    connection.on(
      "PipelineNodeExecutionUpdated",
      (data: { executionId: string; pipelineId: string; nodeId: string; status?: string }) => {
        queryClient.invalidateQueries({
          queryKey: getGetPipelineExecutionQueryKey(data.executionId),
        });
        callbacks?.onNodeExecutionUpdated?.(data.executionId, data.nodeId, data.status);
      }
    );

    connection.on("PipelineExecutionFinished", (data: { executionId: string; pipelineId: string; status: number }) => {
      queryClient.invalidateQueries({
        queryKey: getGetPipelineExecutionsQueryKey(data.pipelineId),
      });
      queryClient.invalidateQueries({
        queryKey: getGetPipelineExecutionQueryKey(data.executionId),
      });
      callbacks?.onExecutionFinished?.(data.executionId);
    });

    async function startConnection() {
      try {
        await connection.start();
        await connection.invoke("JoinPipeline", pipelineId);
      } catch (err) {
        console.warn("SignalR connection failed:", err);
      }
    }

    startConnection();

    return () => {
      if (connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("LeavePipeline", pipelineId).catch(() => {});
      }
      connection.stop().catch(() => {});
    };
  }, [pipelineId, queryClient]);

  return { connection: connectionRef.current };
}
