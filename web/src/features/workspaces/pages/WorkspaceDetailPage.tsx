import { useWorkspaceDetail } from "../hooks/useWorkspaces";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Plus, Loader2, Layers, RefreshCw } from "lucide-react";
import { useDialogStore } from "@/stores/dialogStore";
import { WorkspaceStatsBar } from "../components/WorkspaceStatsBar";
import { WorkspaceResourcesTab } from "../components/tabs/WorkspaceResourcesTab";
import { WorkspaceChangesTab } from "../components/tabs/WorkspaceChangesTab";
import { useResourceQuery, type BaseSearchParams } from "@/lib/useResourceQuery";

interface WorkspaceDetailPageProps {
  projectId: string;
  workspaceId: string;
  useSearch: () => BaseSearchParams & { tab?: "resources" | "changes"; agentId?: string };
  useNavigate: () => any;
}

export function WorkspaceDetailPage({
  projectId,
  workspaceId,
  useSearch,
  useNavigate,
}: WorkspaceDetailPageProps) {
  const search = useSearch();
  const navigate = useNavigate();
  const resource = useResourceQuery(search, navigate);

  const activeTab = search.tab || "resources";

  const { data: workspace, isLoading: isWorkspaceLoading, isError, error } = useWorkspaceDetail(workspaceId);
  const openDialog = useDialogStore((state) => state.openDialog);

  const selectedAgentId = search.agentId || workspace?.workspaceAgents[0]?.agentId || "";

  const handleTabChange = (tab: "resources" | "changes") => {
    resource.onParamChange({ tab, page: 1 } as any, true);
  };

  const handleAgentSelect = (agentId: string) => {
    resource.onParamChange({ agentId, page: 1 } as any, true);
  };

  return (
    <div className="p-6 space-y-6 w-full min-w-0">
      {/* Navigation & Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="icon"
            className="size-9 cursor-pointer"
            onClick={() => navigate({ to: "/projects/$projectId/workspaces", params: { projectId } })}
            aria-label="Back to workspaces"
          >
            <ArrowLeft className="size-4" />
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold tracking-tight text-foreground">
                {isWorkspaceLoading ? "Loading Workspace..." : workspace?.name || "Workspace Detail"}
              </h1>
            </div>
            <p className="text-xs text-muted-foreground mt-0.5">
              Workspace ID: <code className="bg-muted px-1.5 py-0.5 rounded">{workspaceId}</code>
            </p>
          </div>
        </div>

        {/* Tab Navigation & Actions in Header */}
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex items-center p-1 rounded-xl bg-muted/60 border">
            <button
              type="button"
              onClick={() => handleTabChange("resources")}
              className={`inline-flex items-center gap-2 px-4 py-1.5 text-xs font-semibold rounded-lg transition-all cursor-pointer ${activeTab === "resources"
                ? "bg-primary text-primary-foreground shadow-xs"
                : "text-muted-foreground hover:text-foreground"
                }`}
            >
              <Layers className="size-3.5" />
              <span>Overview & Resources</span>
            </button>

            <button
              type="button"
              onClick={() => handleTabChange("changes")}
              className={`inline-flex items-center gap-2 px-4 py-1.5 text-xs font-semibold rounded-lg transition-all cursor-pointer ${activeTab === "changes"
                ? "bg-primary text-primary-foreground shadow-xs"
                : "text-muted-foreground hover:text-foreground"
                }`}
            >
              <RefreshCw className="size-3.5" />
              <span>Sync & Staging</span>
            </button>
          </div>

          <Button
            onClick={() => openDialog("attach-agent-workspace", { workspaceId })}
            variant="outline"
            className="gap-2 text-xs h-9"
          >
            <Plus className="size-3.5" /> Add Agent
          </Button>
        </div>
      </div>

      {/* Loading State */}
      {isWorkspaceLoading && (
        <div className="flex flex-col items-center justify-center py-20 gap-3">
          <Loader2 className="size-8 text-primary animate-spin" />
          <p className="text-sm text-muted-foreground font-medium">Loading workspace details...</p>
        </div>
      )}

      {/* Error State */}
      {isError && (
        <div className="p-6 rounded-xl border border-destructive/30 bg-destructive/10 text-destructive text-center">
          <p className="font-semibold">Failed to load workspace</p>
          <p className="text-xs mt-1">{(error as any)?.message || "An unexpected error occurred."}</p>
        </div>
      )}

      {/* Main Content */}
      {!isWorkspaceLoading && !isError && workspace && (
        <div className="space-y-6">
          {/* Stats Bar */}
          <WorkspaceStatsBar
            agentCount={workspace.agentCount ?? workspace.workspaceAgents.length}
            resourceCount={workspace.resourceCount ?? 0}
            locationCount={workspace.locationCount ?? 0}
          />

          {/* Tab 1: All Synchronized Resources */}
          {activeTab === "resources" && (
            <WorkspaceResourcesTab
              workspaceId={workspaceId}
              projectId={projectId}
              resource={resource}
              search={search}
            />
          )}

          {/* Tab 2: Compare & Sync Local Changes */}
          {activeTab === "changes" && (
            <WorkspaceChangesTab
              workspace={workspace}
              selectedAgentId={selectedAgentId}
              onAgentSelect={handleAgentSelect}
            />
          )}
        </div>
      )}
    </div>
  );
}
