import { useWorkspaceResources } from "../../hooks/useWorkspaceResources";
import { WorkspaceAgentResourceTable } from "../WorkspaceAgentResourceTable";
import type { useResourceQuery, BaseSearchParams } from "@/lib/useResourceQuery";
import type { WorkspaceDetailDto } from "@/gen/model";
import { Bot } from "lucide-react";
import {
  Select,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SelectPopover,
  SelectList,
} from "@/components/ui/select";

interface WorkspaceAgentsTabProps {
  workspace: WorkspaceDetailDto;
  projectId: string;
  selectedAgentId: string;
  onAgentSelect: (agentId: string) => void;
  resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
  search: BaseSearchParams;
}

export function WorkspaceAgentsTab({
  workspace,
  projectId,
  selectedAgentId,
  onAgentSelect,
  resource,
  search,
}: WorkspaceAgentsTabProps) {
  const { data, isLoading } = useWorkspaceResources(
    workspace.id,
    {
      projectId,
      globalKeyword: search.globalKeyword,
      page: search.page,
      pageSize: search.pageSize,
      sort: search.sort,
      filters: search.filters,
    }
  );

  const selectedWorkspaceAgent = workspace.workspaceAgents.find(
    (wa) => wa.agentId === selectedAgentId
  );

  return (
    <div className="space-y-4">
      {/* Agent Selector Card */}
      <div className="p-5 rounded-xl border bg-card shadow-xs space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-2 font-medium text-sm text-foreground">
            <Bot className="size-4 text-primary" />
            <span>Select Connected Agent:</span>
          </div>

          {workspace.workspaceAgents.length > 0 ? (
            <div className="w-full sm:w-72">
              <Select
                selectedKey={selectedAgentId}
                onSelectionChange={(key) => onAgentSelect(String(key))}
                placeholder="Select an Agent"
              >
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectPopover>
                  <SelectList>
                    {workspace.workspaceAgents.map((wa) => (
                      <SelectItem
                        key={wa.agentId}
                        id={wa.agentId}
                        textValue={wa.agent?.name || wa.agentId}
                      >
                        <div className="flex items-center justify-between w-full">
                          <span className="font-medium">{wa.agent?.name || "Agent"}</span>
                          <span className="text-xs text-muted-foreground ml-2">
                            {wa.agent?.isActive ? "🟢 Active" : "🔴 Offline"}
                          </span>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectList>
                </SelectPopover>
              </Select>
            </div>
          ) : (
            <div className="text-xs text-muted-foreground italic">
              No agents attached to this workspace yet.
            </div>
          )}
        </div>

        {/* Selected Agent Details Badge */}
        {selectedWorkspaceAgent && (
          <div className="flex flex-wrap items-center gap-4 pt-3 border-t text-xs text-muted-foreground">
            <div>
              <span className="font-medium text-foreground">Machine:</span>{" "}
              {selectedWorkspaceAgent.agent?.machineKey || "N/A"}
            </div>
            <div>
              <span className="font-medium text-foreground">Root Path:</span>{" "}
              <code className="bg-muted px-1.5 py-0.5 rounded text-[11px]">
                {selectedWorkspaceAgent.rootPath}
              </code>
            </div>
            <div>
              <span className="font-medium text-foreground">Last Seen:</span>{" "}
              {selectedWorkspaceAgent.agent?.lastSeenAt
                ? new Date(selectedWorkspaceAgent.agent.lastSeenAt).toLocaleString()
                : "Never"}
            </div>
          </div>
        )}
      </div>

      {/* Agent Resources Table */}
      <div className="p-6 rounded-xl border bg-card shadow-xs space-y-4">
        <WorkspaceAgentResourceTable
          data={(data?.items || []).map((r) => ({
            resourceId: r.id,
            resourceName: r.displayName || "",
            relativePath: r.relativePath || "",
            versionNo: r.versionCount || 1,
            isOrigin: true,
            discoveredAt: r.createdAt,
            contentId: r.contentId,
            contentName: r.contentName,
            contentTypeName: r.contentTypeName,
            contentTypeColor: r.contentTypeColor,
          }))}
          totalCount={data?.totalCount || 0}
          isLoading={isLoading}
          resource={resource}
        />
      </div>
    </div>
  );
}
