import { useWorkspaceResources } from "../../hooks/useWorkspaceResources";
import { WorkspaceResourceTable } from "../WorkspaceResourceTable";
import type { useResourceQuery, BaseSearchParams } from "@/lib/useResourceQuery";

interface WorkspaceResourcesTabProps {
  workspaceId: string;
  projectId: string;
  resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
  search: BaseSearchParams;
}

export function WorkspaceResourcesTab({
  workspaceId,
  projectId,
  resource,
  search,
}: WorkspaceResourcesTabProps) {
  const { data, isLoading } = useWorkspaceResources(workspaceId, {
    projectId,
    globalKeyword: search.globalKeyword,
    page: search.page,
    pageSize: search.pageSize,
    sort: search.sort,
    filters: search.filters,
  });

  return (
    <div className="p-6 rounded-xl border bg-card shadow-xs space-y-4">
      <WorkspaceResourceTable
        data={data?.items || []}
        totalCount={data?.totalCount || 0}
        isLoading={isLoading}
        resource={resource}
        workspaceId={workspaceId}
        projectId={projectId}
      />
    </div>
  );
}
