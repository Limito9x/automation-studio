import { createContext, useContext, useMemo } from "react";
import type { Node, Edge } from "@xyflow/react";
import type { PipelineVariableDto } from "../hooks/usePipelineGraph";

export interface PipelineFormScopeValue {
  pipelineId?: string;
  projectId?: string;
  variables?: PipelineVariableDto[];
  edges?: Edge[];
  nodes?: Node[];
}

const PipelineFormScopeContext = createContext<PipelineFormScopeValue | null>(null);

export function PipelineFormScopeProvider({
  children,
  value,
}: {
  children: React.ReactNode;
  value: PipelineFormScopeValue;
}) {
  const memoizedValue = useMemo(() => value, [
    value.pipelineId,
    value.projectId,
    value.variables,
    value.edges,
    value.nodes,
  ]);

  return (
    <PipelineFormScopeContext.Provider value={memoizedValue}>
      {children}
    </PipelineFormScopeContext.Provider>
  );
}

export function usePipelineFormScope(): PipelineFormScopeValue {
  const ctx = useContext(PipelineFormScopeContext);
  if (!ctx) {
    throw new Error("usePipelineFormScope must be used within a PipelineFormScopeProvider");
  }
  return ctx;
}
