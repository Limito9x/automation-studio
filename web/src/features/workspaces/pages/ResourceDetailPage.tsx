import { useState, useMemo } from "react";
import { useNavigate } from "@tanstack/react-router";
import type { ResourceVersionDto } from "@/gen/model";
import { useResourceById } from "../hooks/useResourceDetail";
import { ResourceMetadataTab } from "../components/tabs/ResourceMetadataTab";
import { ResourceVersionsTab } from "../components/tabs/ResourceVersionsTab";
import { ResourceTagDropZone } from "../components/ResourceTagDropZone";
import { TagTool } from "@/features/tags/components/TagTool";
import { Button } from "@/components/ui/button";
import {
    ArrowLeft,
    Boxes,
    History,
    HardDrive,
    GitBranch,
} from "lucide-react";

interface ResourceDetailPageProps {
    projectId: string;
    workspaceId?: string;
    resourceId: string;
}

export function ResourceDetailPage({
    projectId,
    workspaceId,
    resourceId,
}: ResourceDetailPageProps) {
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<"metadata" | "versions">("metadata");

    // 1. Single source of truth: Get Resource Detail (includes all versions and metadata)
    const { data: resource, isLoading: isResourceLoading } = useResourceById(resourceId);

    const versions: ResourceVersionDto[] = useMemo(() => {
        return ((resource?.versions || []) as ResourceVersionDto[]).slice().sort((a, b) => b.versionNo - a.versionNo);
    }, [resource?.versions]);
    const latestVersion = versions[0];

    // 2. Selected version state
    const [selectedVersionId, setSelectedVersionId] = useState<string | undefined>(undefined);
    const currentActiveVersionId = selectedVersionId || latestVersion?.id || "";

    const formatBytes = (bytes?: number) => {
        if (!bytes) return "0 B";
        const k = 1024;
        const sizes = ["B", "KB", "MB", "GB", "TB"];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
    };

    const handleBack = () => {
        if (workspaceId) {
            navigate({
                to: "/projects/$projectId/workspaces/$workspaceId",
                params: { projectId, workspaceId },
            });
        } else {
            navigate({
                to: "/projects/$projectId/workspaces",
                params: { projectId },
            });
        }
    };

    return (
        <div className="p-6 space-y-6 w-full min-w-0">
            {/* Tagging Tool on the right dock */}
            <TagTool projectId={projectId} contextTitle={resource?.name || "Resource Detail"} />

            {/* Header matching WorkspaceDetailPage style */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-3 min-w-0">
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={handleBack}
                        className="size-9 shrink-0 cursor-pointer"
                        aria-label="Back to workspace"
                    >
                        <ArrowLeft className="size-4" />
                    </Button>
                    <div className="min-w-0">
                        <div className="flex items-center gap-3 flex-wrap">
                            <h1 className="text-2xl font-bold tracking-tight text-foreground truncate">
                                {isResourceLoading ? "Loading Resource..." : resource?.name || "Resource Detail"}
                            </h1>
                            <ResourceTagDropZone resourceId={resourceId} projectId={projectId} />
                        </div>

                        <p className="text-xs text-muted-foreground mt-0.5 font-mono truncate">
                            Resource ID: <code className="bg-muted px-1.5 py-0.5 rounded">{resourceId}</code>
                            {resource?.filePath && (
                                <span className="ml-2 text-muted-foreground/80">
                                    • Path: <span className="text-primary">{resource.filePath}</span>
                                </span>
                            )}
                        </p>
                    </div>
                </div>

                {/* Tab Navigation in Header */}
                <div className="flex flex-wrap items-center gap-3 shrink-0">
                    <div className="flex items-center p-1 rounded-xl bg-muted/60 border">
                        {/* Tab 1: Metadata & Tagging */}
                        <button
                            type="button"
                            onClick={() => setActiveTab("metadata")}
                            className={`inline-flex items-center gap-2 px-4 py-1.5 text-xs font-semibold rounded-lg transition-all cursor-pointer ${
                                activeTab === "metadata"
                                    ? "bg-primary text-primary-foreground shadow-xs"
                                    : "text-muted-foreground hover:text-foreground"
                            }`}
                        >
                            <Boxes className="size-3.5" />
                            <span>Metadata & Tagging</span>
                        </button>

                        {/* Tab 2: Versions */}
                        <button
                            type="button"
                            onClick={() => setActiveTab("versions")}
                            className={`inline-flex items-center gap-2 px-4 py-1.5 text-xs font-semibold rounded-lg transition-all cursor-pointer ${
                                activeTab === "versions"
                                    ? "bg-primary text-primary-foreground shadow-xs"
                                    : "text-muted-foreground hover:text-foreground"
                            }`}
                        >
                            <History className="size-3.5" />
                            <span>Version History</span>
                        </button>
                    </div>
                </div>
            </div>

            {/* 3 KPI Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {/* Stat 1: Total Versions */}
                <div className="p-4 rounded-xl border bg-card shadow-xs flex items-center gap-3.5">
                    <div className="p-2.5 rounded-lg bg-blue-500/10 text-blue-500 shrink-0">
                        <History className="size-5" />
                    </div>
                    <div>
                        <p className="text-xs font-medium text-muted-foreground">Recorded Versions</p>
                        <h3 className="text-2xl font-bold tracking-tight text-foreground">{versions.length}</h3>
                    </div>
                </div>

                {/* Stat 2: Active Version */}
                <div className="p-4 rounded-xl border bg-card shadow-xs flex items-center gap-3.5">
                    <div className="p-2.5 rounded-lg bg-emerald-500/10 text-emerald-500 shrink-0">
                        <GitBranch className="size-5" />
                    </div>
                    <div>
                        <p className="text-xs font-medium text-muted-foreground">Current Version</p>
                        <h3 className="text-2xl font-bold tracking-tight text-foreground">
                            {latestVersion ? `v${latestVersion.versionNo}.0.0` : "v1.0.0"}
                        </h3>
                    </div>
                </div>

                {/* Stat 3: File Size */}
                <div className="p-4 rounded-xl border bg-card shadow-xs flex items-center gap-3.5">
                    <div className="p-2.5 rounded-lg bg-purple-500/10 text-purple-500 shrink-0">
                        <HardDrive className="size-5" />
                    </div>
                    <div>
                        <p className="text-xs font-medium text-muted-foreground">Resource File Size</p>
                        <h3 className="text-2xl font-bold tracking-tight text-foreground">
                            {formatBytes(latestVersion?.sizeBytes)}
                        </h3>
                    </div>
                </div>
            </div>

            {/* Main Content Area */}
            <div className="p-6 rounded-xl border bg-card shadow-xs">
                {isResourceLoading ? (
                    <div className="space-y-3">
                        {[1, 2, 3].map((i) => (
                            <div key={i} className="h-28 bg-muted/40 animate-pulse rounded-lg border" />
                        ))}
                    </div>
                ) : (
                    <>
                        {activeTab === "metadata" && (
                            <ResourceMetadataTab
                                versions={versions}
                                selectedVersionId={currentActiveVersionId}
                                onSelectVersionId={(verId) => setSelectedVersionId(verId)}
                                projectId={projectId}
                                workspaceId={workspaceId}
                                resourceId={resourceId}
                            />
                        )}

                        {activeTab === "versions" && (
                            <ResourceVersionsTab
                                versions={versions}
                                resourceName={resource?.name}
                                filePath={resource?.filePath || undefined}
                            />
                        )}
                    </>
                )}
            </div>
        </div>
    );
}
