import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SelectPopover,
  SelectList,
} from "@/components/ui/select";
import {
  LocalChangesTable,
  type DiffItemWithStatus,
} from "../LocalChangesTable";
import { MissingResourcesTable } from "../MissingResourcesTable";
import {
  useCompareWorkspace,
  useSyncLocalChanges,
  type DiffResult,
  type WorkspaceDetailDto,
} from "../../hooks/useWorkspaces";
import { toast } from "sonner";
import {
  Bot,
  RefreshCw,
  UploadCloud,
  DownloadCloud,
  CheckCircle2,
  Clock,
  Sparkles,
} from "lucide-react";

interface WorkspaceChangesTabProps {
  workspace: WorkspaceDetailDto;
  selectedAgentId: string;
  onAgentSelect: (agentId: string) => void;
}

export function WorkspaceChangesTab({
  workspace,
  selectedAgentId,
  onAgentSelect,
}: WorkspaceChangesTabProps) {
  const [diffResult, setDiffResult] = useState<DiffResult | null>(null);
  const [selectedPaths, setSelectedPaths] = useState<Set<string>>(new Set());
  const [newResourceNames, setNewResourceNames] = useState<Record<string, string>>({});
  const [activeSubTab, setActiveSubTab] = useState<"local" | "missing">("local");
  const [lastScannedAt, setLastScannedAt] = useState<Date | null>(null);

  const compareMutation = useCompareWorkspace();
  const syncMutation = useSyncLocalChanges(workspace.id);

  const selectedWorkspaceAgent = workspace.workspaceAgents.find(
    (wa) => wa.agentId === selectedAgentId
  );

  // Combine and sort local changes: Added -> Modified -> Deleted
  const localChanges = useMemo<DiffItemWithStatus[]>(() => {
    if (!diffResult) return [];

    const added: DiffItemWithStatus[] = (diffResult.added || []).map((x) => ({
      ...x,
      status: "added",
    }));
    const modified: DiffItemWithStatus[] = (diffResult.modified || []).map((x) => ({
      ...x,
      status: "modified",
    }));
    const deleted: DiffItemWithStatus[] = (diffResult.deleted || []).map((x) => ({
      ...x,
      status: "deleted",
    }));

    return [...added, ...modified, ...deleted];
  }, [diffResult]);

  const missingItems = useMemo(() => {
    return diffResult?.missing || [];
  }, [diffResult]);

  // Trigger compare scan
  const handleScanChanges = async () => {
    if (!selectedAgentId) {
      toast.error("Please select an agent to scan");
      return;
    }

    try {
      const res = await compareMutation.mutateAsync({
        workspaceId: workspace.id,
        agentId: selectedAgentId,
      });

      setDiffResult(res);
      setLastScannedAt(new Date());

      // Auto-select all added & modified items by default for convenience
      const allPaths = new Set([
        ...(res.added || []).map((x) => x.relativePath),
        ...(res.modified || []).map((x) => x.relativePath),
        ...(res.deleted || []).map((x) => x.relativePath),
      ]);
      setSelectedPaths(allPaths);

      toast.success(
        `Scan completed: ${res.added.length} added, ${res.modified.length} modified, ${res.deleted.length} deleted, ${res.missing.length} missing.`
      );
    } catch (err: any) {
      toast.error(err?.message || "Failed to scan workspace agent files.");
    }
  };

  // Toggle selection
  const handleToggleSelect = (path: string) => {
    setSelectedPaths((prev) => {
      const next = new Set(prev);
      if (next.has(path)) {
        next.delete(path);
      } else {
        next.add(path);
      }
      return next;
    });
  };

  const handleToggleSelectAll = () => {
    if (selectedPaths.size === localChanges.length) {
      setSelectedPaths(new Set());
    } else {
      setSelectedPaths(new Set(localChanges.map((x) => x.relativePath)));
    }
  };

  const handleNameChange = (path: string, newName: string) => {
    setNewResourceNames((prev) => ({
      ...prev,
      [path]: newName,
    }));
  };

  // Trigger sync
  const handleSyncSelected = async () => {
    if (selectedPaths.size === 0) {
      toast.warning("Please select at least one item to sync.");
      return;
    }

    try {
      const targetPaths = Array.from(selectedPaths);
      const res = await syncMutation.mutateAsync({
        workspaceId: workspace.id,
        agentId: selectedAgentId,
        data: {
          targetPaths,
          newResourceNames,
          notes: null,
        },
      });

      toast.success(
        `Synchronized successfully: ${res.addedCount} added, ${res.modifiedCount} modified, ${res.locationRemove} locations unlinked.`
      );

      // Re-scan to get refreshed diff
      handleScanChanges();
    } catch (err: any) {
      toast.error(err?.message || "Failed to synchronize changes.");
    }
  };

  return (
    <div className="space-y-6">
      {/* Agent Selector Card (if multiple agents exist) */}
      {workspace.workspaceAgents.length > 1 && (
        <div className="p-4 rounded-xl border bg-card/60 backdrop-blur-xs shadow-2xs flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-2 font-medium text-sm text-foreground">
            <Bot className="size-4 text-primary" />
            <span>Target Agent for Comparison:</span>
          </div>

          <div className="w-full sm:w-72">
            <Select
              selectedKey={selectedAgentId}
              onSelectionChange={(key) => {
                onAgentSelect(String(key));
                setDiffResult(null); // Reset diff when agent changes
              }}
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
        </div>
      )}

      {/* Sync & Staging Status Banner */}
      <div className="p-5 rounded-2xl border bg-card/80 backdrop-blur-md shadow-xs flex flex-col md:flex-row md:items-center justify-between gap-5">
        <div className="flex items-start gap-3.5">
          <div className="size-11 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0 mt-0.5">
            <RefreshCw className={`size-5 ${compareMutation.isPending ? "animate-spin" : ""}`} />
          </div>
          <div className="space-y-1.5">
            <div className="flex flex-wrap items-center gap-2.5">
              <h2 className="text-base font-bold text-foreground">
                Sync & Staging Status
              </h2>
              {diffResult ? (
                <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 border border-emerald-500/30">
                  <CheckCircle2 className="size-3.5" />
                  Scanned ({localChanges.length} local / {missingItems.length} remote)
                </span>
              ) : (
                <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-muted text-muted-foreground border">
                  <Clock className="size-3.5" />
                  Ready to scan
                </span>
              )}
            </div>

            <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
              <span>
                Last Synced:{" "}
                <strong className="text-foreground font-semibold">
                  {selectedWorkspaceAgent?.lastSyncAt
                    ? new Date(selectedWorkspaceAgent.lastSyncAt).toLocaleString()
                    : "Never"}
                </strong>
              </span>
              {lastScannedAt && (
                <>
                  <span className="text-border">|</span>
                  <span>
                    Last Scanned:{" "}
                    <strong className="text-foreground font-semibold">
                      {lastScannedAt.toLocaleTimeString()}
                    </strong>
                  </span>
                </>
              )}
              {selectedWorkspaceAgent?.rootPath && (
                <>
                  <span className="text-border">|</span>
                  <span>
                    Root: <code className="bg-muted px-1.5 py-0.5 rounded text-[11px]">{selectedWorkspaceAgent.rootPath}</code>
                  </span>
                </>
              )}
            </div>
          </div>
        </div>

        {/* Scan Action Button */}
        <div className="shrink-0">
          <Button
            onClick={handleScanChanges}
            isDisabled={compareMutation.isPending || !selectedAgentId}
            className="w-full sm:w-auto gap-2 shadow-xs"
          >
            <RefreshCw className={`size-4 ${compareMutation.isPending ? "animate-spin" : ""}`} />
            <span>{diffResult ? "Re-scan Changes" : "Scan Changes"}</span>
          </Button>
        </div>
      </div>

      {/* When not scanned yet */}
      {!diffResult && !compareMutation.isPending && (
        <div className="p-12 text-center rounded-2xl border border-dashed bg-card/30 flex flex-col items-center justify-center gap-3">
          <div className="size-12 rounded-full bg-primary/10 flex items-center justify-center text-primary">
            <Sparkles className="size-6" />
          </div>
          <div className="max-w-md space-y-1">
            <p className="font-semibold text-foreground text-sm">
              Scan Local Directory for Changes
            </p>
            <p className="text-xs text-muted-foreground">
              Click the <strong>Scan Changes</strong> button above to inspect file differences between your local agent folder and the remote workspace.
            </p>
          </div>
          <Button
            onClick={handleScanChanges}
            isDisabled={!selectedAgentId}
            variant="outline"
            className="mt-2 gap-2"
          >
            <RefreshCw className="size-4" />
            Start Scan
          </Button>
        </div>
      )}

      {/* When scanned: Sub-tabs and Tables */}
      {diffResult && (
        <div className="space-y-4">
          {/* Sub Navigation Bar & Sync Button */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b pb-3">
            {/* Sub Tabs */}
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setActiveSubTab("local")}
                className={`inline-flex items-center gap-2 px-3.5 py-2 text-xs font-semibold rounded-lg transition-colors cursor-pointer ${
                  activeSubTab === "local"
                    ? "bg-primary text-primary-foreground shadow-xs"
                    : "text-muted-foreground hover:text-foreground hover:bg-muted"
                }`}
              >
                <UploadCloud className="size-4" />
                <span>Local vs Workspace</span>
                <span
                  className={`text-[11px] px-1.5 py-0.2 rounded-full ${
                    activeSubTab === "local"
                      ? "bg-primary-foreground/20 text-primary-foreground font-bold"
                      : "bg-muted text-muted-foreground"
                  }`}
                >
                  {localChanges.length}
                </span>
              </button>

              <button
                type="button"
                onClick={() => setActiveSubTab("missing")}
                className={`inline-flex items-center gap-2 px-3.5 py-2 text-xs font-semibold rounded-lg transition-colors cursor-pointer ${
                  activeSubTab === "missing"
                    ? "bg-primary text-primary-foreground shadow-xs"
                    : "text-muted-foreground hover:text-foreground hover:bg-muted"
                }`}
              >
                <DownloadCloud className="size-4" />
                <span>Pull from Remote</span>
                <span
                  className={`text-[11px] px-1.5 py-0.2 rounded-full ${
                    activeSubTab === "missing"
                      ? "bg-primary-foreground/20 text-primary-foreground font-bold"
                      : "bg-muted text-muted-foreground"
                  }`}
                >
                  {missingItems.length}
                </span>
              </button>
            </div>

            {/* Sync Action Button (Visible on Local tab) */}
            {activeSubTab === "local" && (
              <Button
                onClick={handleSyncSelected}
                isDisabled={syncMutation.isPending || selectedPaths.size === 0}
                className="gap-2 shadow-xs bg-emerald-600 hover:bg-emerald-700 text-white"
              >
                <UploadCloud className={`size-4 ${syncMutation.isPending ? "animate-spin" : ""}`} />
                <span>
                  Sync to Workspace ({selectedPaths.size} selected)
                </span>
              </Button>
            )}
          </div>

          {/* Sub Tab Contents */}
          {activeSubTab === "local" && (
            <LocalChangesTable
              items={localChanges}
              selectedPaths={selectedPaths}
              onToggleSelect={handleToggleSelect}
              onToggleSelectAll={handleToggleSelectAll}
              newResourceNames={newResourceNames}
              onNameChange={handleNameChange}
            />
          )}

          {activeSubTab === "missing" && (
            <MissingResourcesTable items={missingItems} />
          )}
        </div>
      )}
    </div>
  );
}
