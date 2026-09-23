import { useWorkspaces, type WorkspaceDto } from "../hooks/useWorkspaces";
import { WorkspaceCard } from "../components/workspace-card";
import { Button } from "@/components/ui/button";
import { Plus, Layers, Loader2 } from "lucide-react";
import { useDialogStore } from "@/stores/dialogStore";

interface WorkspaceListPageProps {
  projectId: string;
}

export function WorkspaceListPage({ projectId }: WorkspaceListPageProps) {
  const { data, isLoading, isError, error } = useWorkspaces(projectId);
  const workspaces = (data as unknown as WorkspaceDto[]) || [];
  const openDialog = useDialogStore((state) => state.openDialog);

  return (
    <div className="p-6 mx-auto space-y-6 w-full min-w-0">

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Workspaces</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Manage environments and agent connections for this project.
          </p>
        </div>
        <Button
          onPress={() => openDialog("create-workspace", { projectId })}
          className="flex items-center gap-2"
        >
          <Plus className="size-4" />
          <span>New Workspace</span>
        </Button>
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-16 text-muted-foreground">
          <Loader2 className="size-6 animate-spin mr-2" />
          <span>Loading workspaces...</span>
        </div>
      )}

      {/* Error state */}
      {isError && (
        <div className="p-4 rounded-lg bg-destructive/10 text-destructive text-sm">
          Failed to load workspaces: {(error as any)?.message || "Unknown error"}
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !isError && workspaces.length === 0 && (
        <div className="flex flex-col items-center justify-center py-16 px-4 rounded-xl border border-dashed text-center bg-card">
          <div className="p-3 rounded-full bg-primary/10 text-primary mb-3">
            <Layers className="size-8" />
          </div>
          <h3 className="text-base font-semibold">No workspaces yet</h3>
          <p className="text-sm text-muted-foreground max-w-sm mt-1 mb-4">
            Create your first workspace to connect agents and organize your automation resources.
          </p>
          <Button
            size="sm"
            onPress={() => openDialog("create-workspace", { projectId })}
          >
            <Plus className="size-4 mr-1.5" />
            <span>Create Workspace</span>
          </Button>
        </div>
      )}

      {/* Workspace Card Grid */}
      {!isLoading && !isError && workspaces.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
          {workspaces.map((workspace) => (
            <WorkspaceCard key={workspace.id} workspace={workspace} />
          ))}
        </div>
      )}
    </div>
  );
}
