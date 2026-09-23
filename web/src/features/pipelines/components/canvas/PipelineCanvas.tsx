import { useState, useCallback, useMemo, useEffect } from "react";
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  addEdge,
  BackgroundVariant,
  useReactFlow,
  MarkerType,
} from "@xyflow/react";
import type { Connection, Edge, Node } from "@xyflow/react";
import "@xyflow/react/dist/style.css";

import { CustomPipelineNode } from "./CustomPipelineNode";
import type { CustomPipelineNodeData } from "./CustomPipelineNode";
import { ContextMenuPalette } from "./ContextMenuPalette";
import { NodeConfigInspector } from "./NodeConfigInspector";
import { CanvasToolbar } from "./CanvasToolbar";
import { VariablePanel } from "./VariablePanel";
import { VariableDropMenu } from "./VariableDropMenu";
import { RunPipelineModal } from "../../dialogs/RunPipelineModal";
import { LiveExecutionDrawer } from "./LiveExecutionDrawer";
import { PipelineFormScopeProvider } from "../../form-scope/PipelineFormScope";
import {
  useAddPipelineNode,
  useUpdatePipelineNode,
  useUpdateNodePosition,
  useDeletePipelineNode,
  useAddPipelineEdge,
  useDeletePipelineEdge,
  useValidatePipeline,
  usePipelineExecution,
  usePipelineExecutions,
} from "../../hooks/usePipelineGraph";
import { usePipelineSignalR } from "../../hooks/usePipelineSignalR";
import type { ExtendedPipelineGraphDto } from "../../hooks/usePipelineGraph";
import type { NodePaletteItemDto } from "@/gen/model";
import { toast } from "sonner";

interface PipelineCanvasProps {
  projectId: string;
  graph: ExtendedPipelineGraphDto;
}

const nodeTypes = {
  pipelineNode: CustomPipelineNode,
};

const defaultEdgeOptions = {
  animated: true,
  style: { strokeWidth: 2 },
};

export function PipelineCanvas({ projectId, graph }: PipelineCanvasProps) {
  const { screenToFlowPosition } = useReactFlow();

  // Convert graph DTO to React Flow nodes
  const initialNodes: Node[] = useMemo(() => {
    return (graph.nodes || []).map((n) => {
      const isStart = n.kind?.toLowerCase() === "start" || n.refId?.toLowerCase() === "start";
      return {
        id: n.id,
        type: "pipelineNode",
        deletable: !isStart,
        position: { x: n.position.x, y: n.position.y },
        data: {
          refId: n.refId,
          kind: n.kind,
          label: n.label,
          category: n.category,
          executor: n.executor,
          inputs: n.inputs || [],
          outputs: n.outputs || [],
          configValues: n.configValues || {},
          pipelineId: graph.id,
        } as CustomPipelineNodeData,
      };
    });
  }, [graph.nodes]);

  // Helper to determine if a handle is Exec Flow
  const isExecHandle = (handleId?: string | null) => {
    if (!handleId) return false;
    return (
      handleId === "exec_in" ||
      handleId === "exec_out" ||
      handleId === "loop_body" ||
      handleId === "completed"
    );
  };

  // Helper to determine if edge is Exec Flow
  const isExecEdge = (sourcePin?: string | null, targetPin?: string | null, kind?: any) => {
    return (
      kind === 1 ||
      kind === "Exec" ||
      isExecHandle(sourcePin) ||
      isExecHandle(targetPin)
    );
  };

  // Convert graph DTO to React Flow edges
  const initialEdges: Edge[] = useMemo(() => {
    return (graph.edges || []).map((e) => {
      const isExec = isExecEdge(e.sourcePin, e.targetPin, e.kind);
      return {
        id: e.id,
        source: e.sourceNodeId,
        sourceHandle: e.sourcePin,
        target: e.targetNodeId,
        targetHandle: e.targetPin,
        animated: !isExec,
        markerEnd: {
          type: MarkerType.ArrowClosed,
          color: isExec ? "currentColor" : "#38bdf8",
          width: 16,
          height: 16,
        },
        className: isExec ? "text-foreground" : "text-sky-400",
        style: isExec
          ? { stroke: "currentColor", strokeWidth: 3.5, strokeDasharray: "none" }
          : { stroke: "#38bdf8", strokeWidth: 2 },
      };
    });
  }, [graph.edges]);

  const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes);
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);

  // Sync if pipeline graph changes
  useEffect(() => {
    setNodes(initialNodes);
    setEdges(initialEdges);
  }, [graph.id, initialNodes, initialEdges, setNodes, setEdges]);

  // Selected Node for Config Inspector
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const selectedNode = useMemo(() => nodes.find((n) => n.id === selectedNodeId) || null, [nodes, selectedNodeId]);

  // Right-click Context Palette Menu State
  const [palettePosition, setPalettePosition] = useState<{ x: number; y: number } | null>(null);
  const [flowCoordinates, setFlowCoordinates] = useState<{ x: number; y: number }>({ x: 100, y: 100 });

  // Run Modal & Live Execution Drawer State
  const [isRunModalOpen, setIsRunModalOpen] = useState(false);
  const [activeExecutionId, setActiveExecutionId] = useState<string | null>(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [drawerDefaultTab, setDrawerDefaultTab] = useState<"history" | "inspect" | "logs">("logs");

  // Live Execution tracking & SignalR
  const { data: executions } = usePipelineExecutions(graph.id);
  const currentExecId = activeExecutionId || executions?.[0]?.id;
  const { data: currentExecution } = usePipelineExecution(currentExecId || undefined);
  const [liveNodeStatuses, setLiveNodeStatuses] = useState<Record<string, "idle" | "running" | "succeeded" | "failed">>({});

  usePipelineSignalR(graph.id, {
    onExecutionStarted: (execId) => {
      setActiveExecutionId(execId);
      setLiveNodeStatuses({});
    },
    onNodeExecutionUpdated: (_execId, nodeId, status) => {
      if (status) {
        const s = status.toLowerCase();
        const mappedStatus = s === "running" ? "running" : s === "failed" ? "failed" : "succeeded";
        setLiveNodeStatuses((prev) => ({
          ...prev,
          [nodeId]: mappedStatus,
        }));
      }
    },
    onExecutionFinished: () => {
      // Finished
    },
  });

  // Real-time synchronization of executionStatus into React Flow nodes
  useEffect(() => {
    if (!currentExecution) return;

    let stateObj: any = null;
    try {
      if (typeof currentExecution.executionState === "string") {
        stateObj = JSON.parse(currentExecution.executionState);
      } else {
        stateObj = currentExecution.executionState;
      }
    } catch {
      stateObj = null;
    }

    const nodeOutputs = stateObj?.NodeOutputs || {};
    const execStatus = Number(currentExecution.status);
    const isFailed = execStatus === 5;

    setNodes((currentNodes) => {
      return currentNodes.map((node) => {
        let newStatus: "idle" | "running" | "succeeded" | "failed" = "idle";
        let newError: string | null = null;

        if (nodeOutputs[node.id]) {
          newStatus = "succeeded";
        } else if (liveNodeStatuses[node.id]) {
          newStatus = liveNodeStatuses[node.id];
          if (newStatus === "failed") {
            newError = currentExecution.errorMessage;
          }
        } else if (isFailed && prevExecutionStatus(node) === "running") {
          newStatus = "failed";
          newError = currentExecution.errorMessage;
        }

        const prevData = node.data as CustomPipelineNodeData;
        if (prevData.executionStatus === newStatus && prevData.executionError === newError) {
          return node;
        }

        return {
          ...node,
          data: {
            ...prevData,
            executionStatus: newStatus,
            executionError: newError,
          },
        };
      });
    });
  }, [currentExecution, liveNodeStatuses, setNodes]);

  function prevExecutionStatus(node: Node): string | undefined {
    return (node.data as CustomPipelineNodeData)?.executionStatus;
  }
  const [isVariablesOpen, setIsVariablesOpen] = useState(true);
  const [varDropState, setVarDropState] = useState<{
    varName: string;
    screenPos: { x: number; y: number };
    flowPos: { x: number; y: number };
  } | null>(null);

  // Granular CRUD Mutations
  const addNodeMutation = useAddPipelineNode(graph.id);
  const updateNodeMutation = useUpdatePipelineNode(graph.id);
  const updateNodePositionMutation = useUpdateNodePosition(graph.id);
  const deleteNodeMutation = useDeletePipelineNode(graph.id);
  const addEdgeMutation = useAddPipelineEdge(graph.id);
  const deleteEdgeMutation = useDeletePipelineEdge(graph.id);
  const validateMutation = useValidatePipeline(graph.id);

  // Listen to node config updates dispatched from canvas components
  useEffect(() => {
    const handleCustomUpdate = (e: Event) => {
      const { nodeId, configValues } = (e as CustomEvent).detail;
      updateNodeMutation.mutate({
        nodeId,
        data: { configValues },
      });
    };
    window.addEventListener("pipeline:update-node-config", handleCustomUpdate);
    return () => window.removeEventListener("pipeline:update-node-config", handleCustomUpdate);
  }, [updateNodeMutation]);

  // Strict Connection Validation
  const isValidConnection = useCallback(
    (connection: Connection | Edge) => {
      // 1. Cannot connect to self
      if (connection.source === connection.target) return false;
      if (!connection.sourceHandle || !connection.targetHandle) return false;

      const isSourceExec = isExecHandle(connection.sourceHandle);
      const isTargetExec = isExecHandle(connection.targetHandle);

      // 2. Exec flow can only connect to Exec flow
      if (isSourceExec || isTargetExec) {
        return isSourceExec && isTargetExec;
      }

      // 3. For Data pins: target cannot receive multiple wires if Single cardinality
      const targetNode = nodes.find((n) => n.id === connection.target);
      const targetPin = (targetNode?.data as any)?.inputs?.find(
        (p: any) => p.id === connection.targetHandle
      );
      const isArray = targetPin?.cardinality === 1 || targetPin?.cardinality === "Array";

      if (!isArray) {
        const alreadyConnected = edges.some(
          (e) => e.target === connection.target && e.targetHandle === connection.targetHandle
        );
        if (alreadyConnected) return false;
      }

      return true;
    },
    [nodes, edges]
  );

  // Connect Handler (Wired an edge) -> Calls POST /api/pipelines/{id}/edges
  const onConnect = useCallback(
    async (params: Connection) => {
      if (!params.source || !params.target || !params.sourceHandle || !params.targetHandle) return;

      const isExec = isExecEdge(params.sourceHandle, params.targetHandle);

      try {
        const createdEdge = await addEdgeMutation.mutateAsync({
          sourcePipelineNodeId: params.source,
          sourcePin: params.sourceHandle,
          targetPipelineNodeId: params.target,
          targetPin: params.targetHandle,
        });

        setEdges((eds) => {
          // If connecting ExecOut (1-to-1 rule), remove existing edge from this source exec handle
          const filtered = isExec
            ? eds.filter((e) => !(e.source === params.source && e.sourceHandle === params.sourceHandle))
            : eds;

          return addEdge(
            {
              ...params,
              id: createdEdge.id,
              animated: !isExec,
              markerEnd: {
                type: MarkerType.ArrowClosed,
                color: isExec ? "currentColor" : "#38bdf8",
                width: 16,
                height: 16,
              },
              className: isExec ? "text-foreground" : "text-sky-400",
              style: isExec
                ? { stroke: "currentColor", strokeWidth: 3.5, strokeDasharray: "none" }
                : { stroke: "#38bdf8", strokeWidth: 2 },
            },
            filtered
          );
        });

        // Auto-Infer Struct Type for BreakStruct node when wired to Target pin
        const targetNode = nodes.find((n) => n.id === params.target);
        const isTargetBreakStruct = (targetNode?.data as any)?.refId?.toLowerCase() === "breakstruct";
        if (isTargetBreakStruct && (params.targetHandle === "Target" || params.targetHandle === "target")) {
          const sourceNode = nodes.find((n) => n.id === params.source);
          const sourcePins = (sourceNode?.data as any)?.outputs || [];
          const sourcePin = sourcePins.find((p: any) => p.id === params.sourceHandle);
          const pinText = `${sourcePin?.id || ""} ${sourcePin?.label || ""} ${(sourcePin?.metadata as any) || ""}`.toLowerCase();

          let inferredStructType = "Resource";
          if (pinText.includes("inspection")) inferredStructType = "Inspection";
          else if (pinText.includes("workspace")) inferredStructType = "Workspace";
          else if (pinText.includes("resource")) inferredStructType = "Resource";
          else if (sourcePin?.metadata && typeof sourcePin.metadata === "string" && sourcePin.metadata.trim()) {
            inferredStructType = sourcePin.metadata.trim();
          }

          setNodes((nds) =>
            nds.map((n) => {
              if (n.id !== params.target) return n;
              const nodeData = n.data as any;
              const currentConfig = nodeData.configValues || {};
              const updatedConfig = { ...currentConfig, StructType: inferredStructType };

              updateNodeMutation.mutate({
                nodeId: params.target,
                data: {
                  configValues: updatedConfig,
                },
              });

              return {
                ...n,
                data: {
                  ...n.data,
                  configValues: updatedConfig,
                },
              };
            })
          );
        }
      } catch {
        // Handled by toast
      }
    },
    [addEdgeMutation, setEdges, nodes, updateNodeMutation, setNodes]
  );

  // Drag stop handler (Node moved) -> Calls PATCH /api/pipelines/{id}/nodes/{nodeId} without refetching graph query
  const onNodeDragStop = useCallback(
    (_: MouseEvent | TouchEvent, node: Node) => {
      updateNodePositionMutation.mutate({
        nodeId: node.id,
        data: {
          positionX: node.position.x,
          positionY: node.position.y,
        },
      });
    },
    [updateNodePositionMutation]
  );

  // Nodes deleted handler (Delete key or backspace) -> Calls DELETE /api/pipelines/{id}/nodes/{nodeId}
  const onNodesDelete = useCallback(
    (deletedNodes: Node[]) => {
      const filtered = deletedNodes.filter(
        (n) => (n.data as any)?.kind?.toLowerCase() !== "start" && (n.data as any)?.refId?.toLowerCase() !== "start"
      );
      filtered.forEach((n) => {
        deleteNodeMutation.mutate(n.id);
      });
      if (selectedNodeId && filtered.some((n) => n.id === selectedNodeId)) {
        setSelectedNodeId(null);
      }
    },
    [deleteNodeMutation, selectedNodeId]
  );

  // Edges deleted handler (Delete key or backspace on selected wire) -> Calls DELETE /api/pipelines/{id}/edges/{edgeId}
  const onEdgesDelete = useCallback(
    (deletedEdges: Edge[]) => {
      deletedEdges.forEach((e) => {
        deleteEdgeMutation.mutate(e.id);
      });
      toast.info("Connection deleted");
    },
    [deleteEdgeMutation]
  );

  // Right-click on Edge to immediately cut/delete the connection wire
  const onEdgeContextMenu = useCallback(
    (event: React.MouseEvent, edge: Edge) => {
      event.preventDefault();
      setEdges((eds) => eds.filter((e) => e.id !== edge.id));
      deleteEdgeMutation.mutate(edge.id);
      toast.info("Connection deleted");
    },
    [deleteEdgeMutation, setEdges]
  );

  // Pane Context Menu (Right Click on Canvas) -> Opens Palette
  const onPaneContextMenu = useCallback(
    (event: React.MouseEvent | MouseEvent) => {
      event.preventDefault();
      const coords = screenToFlowPosition({
        x: event.clientX,
        y: event.clientY,
      });
      setFlowCoordinates(coords);
      setPalettePosition({ x: event.clientX, y: event.clientY });
    },
    [screenToFlowPosition]
  );

  // Add Node from Palette -> Calls POST /api/pipelines/{id}/nodes immediately
  const handleSelectPaletteItem = useCallback(
    async (item: NodePaletteItemDto) => {
      try {
        const createdNode = await addNodeMutation.mutateAsync({
          refId: item.key,
          kind: item.source || "Tool",
          positionX: flowCoordinates.x,
          positionY: flowCoordinates.y,
          configValues: {},
        });

        const newNode: Node = {
          id: createdNode.id,
          type: "pipelineNode",
          position: { x: createdNode.position.x, y: createdNode.position.y },
          data: {
            refId: createdNode.refId,
            kind: createdNode.kind,
            label: createdNode.label,
            category: createdNode.category,
            executor: createdNode.executor,
            inputs: createdNode.inputs || [],
            outputs: createdNode.outputs || [],
            configValues: createdNode.configValues || {},
            pipelineId: graph.id,
          } as CustomPipelineNodeData,
        };

        setNodes((nds) => [...nds, newNode]);
        setSelectedNodeId(createdNode.id);
      } catch {
        // Handled by toast
      }
    },
    [flowCoordinates, addNodeMutation, setNodes]
  );

  // Spawn Get/Set Variable Node at specific coordinates
  const handleSpawnVariableNodeAt = useCallback(
    async (
      toolKey: "GetVariable" | "SetVariable",
      varName: string,
      position: { x: number; y: number }
    ) => {
      try {
        const isGet = toolKey === "GetVariable";
        const label = isGet ? `Get ${varName}` : `Set ${varName}`;
        const createdNode = await addNodeMutation.mutateAsync({
          refId: toolKey,
          kind: "Tool",
          positionX: position.x,
          positionY: position.y,
          configValues: {
            VariableName: varName,
          },
        });

        const newNode: Node = {
          id: createdNode.id,
          type: "pipelineNode",
          position: { x: createdNode.position.x, y: createdNode.position.y },
          data: {
            refId: createdNode.refId,
            kind: createdNode.kind,
            label: createdNode.label || label,
            category: createdNode.category || "Variables",
            executor: createdNode.executor || "builtin",
            inputs: createdNode.inputs || [],
            outputs: createdNode.outputs || [],
            configValues: createdNode.configValues || { VariableName: varName },
            pipelineId: graph.id,
          } as CustomPipelineNodeData,
        };

        setNodes((nds) => [...nds, newNode]);
        setSelectedNodeId(createdNode.id);
        toast.success(`Spawned '${label}' node`);
      } catch (err: any) {
        toast.error(err?.message || `Failed to spawn ${toolKey}`);
      }
    },
    [addNodeMutation, graph.id, setNodes]
  );

  // Spawn Get/Set Variable Node directly from VariablePanel button
  const handleSpawnVariableNode = useCallback(
    (toolKey: "GetVariable" | "SetVariable", varName: string) => {
      handleSpawnVariableNodeAt(toolKey, varName, {
        x: 300 + Math.random() * 60,
        y: 250 + Math.random() * 60,
      });
    },
    [handleSpawnVariableNodeAt]
  );

  // Drag over Canvas handler
  const onDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  }, []);

  // Drop Variable onto Canvas handler (Unreal-style)
  const onDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      const rawVar = e.dataTransfer.getData("application/pipeline-variable");
      if (!rawVar) return;

      try {
        const varData = JSON.parse(rawVar);
        const varName = varData.name;
        if (!varName) return;

        const flowPos = screenToFlowPosition({ x: e.clientX, y: e.clientY });

        if (e.ctrlKey) {
          // Ctrl+Drop -> Immediately spawn Get
          handleSpawnVariableNodeAt("GetVariable", varName, flowPos);
        } else if (e.altKey) {
          // Alt+Drop -> Immediately spawn Set
          handleSpawnVariableNodeAt("SetVariable", varName, flowPos);
        } else {
          // Normal Drop -> Show sleek Unreal popup menu right at cursor
          setVarDropState({
            varName,
            screenPos: { x: e.clientX, y: e.clientY },
            flowPos,
          });
        }
      } catch {
        // Ignore malformed drop
      }
    },
    [screenToFlowPosition, handleSpawnVariableNodeAt]
  );

  // Update Config for an unwired input pin -> Calls PATCH /api/pipelines/{id}/nodes/{nodeId}
  const handleUpdateConfig = useCallback(
    (nodeId: string, pinId: string, value: any) => {
      setNodes((nds) =>
        nds.map((n) => {
          if (n.id !== nodeId) return n;
          const nodeData = n.data as any;
          const currentConfig = nodeData.configValues || {};
          const updatedConfig = { ...currentConfig, [pinId]: value };

          updateNodeMutation.mutate({
            nodeId,
            data: {
              configValues: updatedConfig,
            },
          });

          return {
            ...n,
            data: {
              ...n.data,
              configValues: updatedConfig,
            },
          };
        })
      );
    },
    [setNodes, updateNodeMutation]
  );

  // Delete Node from inspector -> Calls DELETE /api/pipelines/{id}/nodes/{nodeId}
  const handleDeleteNode = useCallback(
    (nodeId: string) => {
      deleteNodeMutation.mutate(nodeId);
      setNodes((nds) => nds.filter((n) => n.id !== nodeId));
      setEdges((eds) => eds.filter((e) => e.source !== nodeId && e.target !== nodeId));
      if (selectedNodeId === nodeId) {
        setSelectedNodeId(null);
      }
    },
    [deleteNodeMutation, selectedNodeId, setNodes, setEdges]
  );

  // Validate Pipeline
  const handleValidate = async () => {
    try {
      const res = await validateMutation.mutateAsync({
        runtimeInputs: {},
      });
      if (res.isValid) {
        toast.success("Pipeline graph is valid with no unresolved dependencies.");
      } else {
        if (res.cycleNodeIds && res.cycleNodeIds.length > 0) {
          toast.error(`Pipeline contains a cycle involving nodes: ${res.cycleNodeIds.join(", ")}`);
        } else if (res.unresolvedPins && res.unresolvedPins.length > 0) {
          toast.error(`Unresolved required pins: ${res.unresolvedPins.map((p) => p.pinLabel || p.pinKey).join(", ")}`);
        }
      }
    } catch (err: any) {
      toast.error(err?.message || "Validation failed");
    }
  };

  const isMutating =
    addNodeMutation.isPending ||
    updateNodeMutation.isPending ||
    deleteNodeMutation.isPending ||
    addEdgeMutation.isPending ||
    deleteEdgeMutation.isPending;

  const scopeValue = useMemo(
    () => ({
      pipelineId: graph.id,
      projectId,
      variables: graph.variables || [],
      edges,
      nodes,
    }),
    [graph.id, projectId, graph.variables, edges, nodes]
  );

  return (
    <PipelineFormScopeProvider value={scopeValue}>
      <div className="relative flex h-full w-full flex-col overflow-hidden bg-background">
        {/* Top Toolbar */}
        <CanvasToolbar
          projectId={projectId}
          pipelineId={graph.id}
          pipelineName={graph.name}
          triggerType={graph.triggerType}
          isSaving={isMutating}
          onOpenRunModal={() => setIsRunModalOpen(true)}
          onOpenHistory={() => {
            setDrawerDefaultTab("history");
            setIsDrawerOpen(true);
          }}
          onValidate={handleValidate}
          isValidating={validateMutation.isPending}
        />

        {/* Main Canvas & Inspector Layout */}
        <div className="relative flex flex-1 overflow-hidden">
          <div className="relative flex-1 h-full">
            <ReactFlow
              nodes={nodes}
              edges={edges}
              onNodesChange={onNodesChange}
              onEdgesChange={onEdgesChange}
              onNodeDragStop={onNodeDragStop}
              onNodesDelete={onNodesDelete}
              onEdgesDelete={onEdgesDelete}
              onEdgeContextMenu={onEdgeContextMenu}
              onConnect={onConnect}
              isValidConnection={isValidConnection}
              nodeTypes={nodeTypes}
              defaultEdgeOptions={defaultEdgeOptions}
              deleteKeyCode={["Backspace", "Delete"]}
              onNodeClick={(_, node) => setSelectedNodeId(node.id)}
              onPaneClick={() => setSelectedNodeId(null)}
              onPaneContextMenu={onPaneContextMenu}
              onDragOver={onDragOver}
              onDrop={onDrop}
              fitView
              fitViewOptions={{ padding: 0.2 }}
              onlyRenderVisibleElements={true}
              proOptions={{ hideAttribution: true }}
              className="bg-dot-grid"
            >
              <Background variant={BackgroundVariant.Dots} gap={16} size={1} className="opacity-40" />
              <Controls />
              <MiniMap
                zoomable
                pannable
                nodeColor={(node) => {
                  const kind = (node.data as any)?.kind?.toLowerCase();
                  if (kind === "start") return "#10b981";
                  if (kind === "tool") return "#3b82f6";
                  return "#a855f7";
                }}
              />
            </ReactFlow>

            {/* Right-click Context Palette Popover */}
            <ContextMenuPalette
              projectId={projectId}
              position={palettePosition}
              onClose={() => setPalettePosition(null)}
              onSelect={handleSelectPaletteItem}
            />

            {/* Unreal-style Variable Drop Context Menu */}
            {varDropState && (
              <VariableDropMenu
                varName={varDropState.varName}
                position={varDropState.screenPos}
                onSelect={(action) => {
                  const toolKey = action === "Get" ? "GetVariable" : "SetVariable";
                  handleSpawnVariableNodeAt(toolKey, varDropState.varName, varDropState.flowPos);
                  setVarDropState(null);
                }}
                onClose={() => setVarDropState(null)}
              />
            )}

            {/* Left-side Blackboard Variables Panel */}
            <VariablePanel
              pipelineId={graph.id}
              variables={graph.variables || []}
              onSpawnNode={handleSpawnVariableNode}
              isOpen={isVariablesOpen}
              onToggle={() => setIsVariablesOpen((prev) => !prev)}
            />

            {/* Live Execution Run & History Drawer */}
            {isDrawerOpen && (
              <LiveExecutionDrawer
                pipelineId={graph.id}
                executionId={activeExecutionId}
                defaultTab={drawerDefaultTab}
                onSelectExecution={(id) => {
                  setActiveExecutionId(id);
                  setDrawerDefaultTab("inspect");
                }}
                onClose={() => setIsDrawerOpen(false)}
              />
            )}
          </div>

          {/* Selected Node Config Inspector Side Panel */}
          {selectedNode && (
            <NodeConfigInspector
              pipelineId={graph.id}
              node={selectedNode}
              triggerType={graph.triggerType}
              triggerWorkspaceId={graph.triggerWorkspaceId}
              triggerConfig={graph.triggerConfig}
              onClose={() => setSelectedNodeId(null)}
              onUpdateConfig={handleUpdateConfig}
              onDeleteNode={handleDeleteNode}
            />
          )}
        </div>

        {/* Run Pipeline Modal */}
        {isRunModalOpen && (
          <RunPipelineModal
            pipelineId={graph.id}
            pipelineName={graph.name}
            projectId={projectId}
            nodes={nodes}
            edges={edges}
            isOpen={isRunModalOpen}
            onClose={() => setIsRunModalOpen(false)}
            onExecutionStarted={(execId) => {
              setActiveExecutionId(execId);
              setDrawerDefaultTab("logs");
              setIsDrawerOpen(true);
            }}
          />
        )}
      </div>
    </PipelineFormScopeProvider>
  );
}
