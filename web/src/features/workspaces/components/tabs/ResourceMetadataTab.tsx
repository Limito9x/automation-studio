import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { FileJson, Copy, Check, Braces, Table2 } from "lucide-react";
import type { ResourceVersionDto } from "@/gen/model";
import { JsonTreeTable } from "@/components/custom-ui/tables/JsonTreeTable";
import { cn } from "@/lib/utils";

interface ResourceMetadataTabProps {
    versions: ResourceVersionDto[];
    selectedVersionId: string;
    onSelectVersionId: (versionId: string) => void;
    projectId?: string;
    workspaceId?: string;
    resourceId?: string;
}

export function ResourceMetadataTab({
    versions,
    selectedVersionId,
    onSelectVersionId,
}: ResourceMetadataTabProps) {
    const [copiedJson, setCopiedJson] = useState(false);
    const [viewMode, setViewMode] = useState<"tree" | "raw">("tree");

    const currentVersion = useMemo(() => {
        return versions.find((v) => v.id === selectedVersionId) || versions[0];
    }, [versions, selectedVersionId]);

    // Parse metadata object
    const metadata = useMemo(() => {
        if (!currentVersion?.metadata) return null;
        let data: any = currentVersion.metadata;
        if (typeof data === "string") {
            try {
                data = JSON.parse(data);
            } catch {
                return null;
            }
        }
        if (typeof data === "string") {
            try {
                data = JSON.parse(data);
            } catch {
                // ignore
            }
        }
        if (data && typeof data === "object" && "rootElement" in data) {
            data = data.rootElement;
        }
        return data;
    }, [currentVersion]);

    const handleCopyJson = () => {
        if (!metadata) return;
        navigator.clipboard.writeText(JSON.stringify(metadata, null, 2));
        setCopiedJson(true);
        setTimeout(() => setCopiedJson(false), 2000);
    };

    const tagsByPath = currentVersion?.tagsByPath || {};

    return (
        <div className="space-y-4">
            {/* Toolbar: Version Selector & Actions */}
            <div className="flex flex-wrap items-center justify-between gap-3 p-3 bg-muted/20 border rounded-xl">
                <div className="flex items-center gap-1.5 flex-wrap">
                    <span className="text-xs font-semibold text-muted-foreground mr-1">Version:</span>
                    {versions.map((v) => (
                        <button
                            key={v.id}
                            type="button"
                            onClick={() => onSelectVersionId(v.id)}
                            className={cn(
                                "px-3 py-1 text-xs font-mono rounded-lg border transition-all cursor-pointer",
                                v.id === currentVersion?.id
                                    ? "bg-primary text-primary-foreground border-primary shadow-xs font-bold"
                                    : "bg-card text-muted-foreground hover:text-foreground border-border"
                            )}
                        >
                            v{v.versionNo}
                        </button>
                    ))}
                </div>

                {metadata && (
                    <div className="flex items-center gap-2 flex-wrap">
                        <div className="flex items-center p-0.5 rounded-lg bg-muted border">
                            <button
                                type="button"
                                onClick={() => setViewMode("tree")}
                                className={cn(
                                    "px-3 py-1 text-xs font-medium rounded-md transition-all cursor-pointer flex items-center gap-1.5",
                                    viewMode === "tree"
                                        ? "bg-background text-foreground shadow-2xs font-semibold"
                                        : "text-muted-foreground hover:text-foreground"
                                )}
                            >
                                <Table2 className="size-3.5 text-primary" />
                                <span>Table View</span>
                            </button>
                            <button
                                type="button"
                                onClick={() => setViewMode("raw")}
                                className={cn(
                                    "px-3 py-1 text-xs font-medium rounded-md transition-all cursor-pointer flex items-center gap-1.5",
                                    viewMode === "raw"
                                        ? "bg-background text-foreground shadow-2xs font-semibold"
                                        : "text-muted-foreground hover:text-foreground"
                                )}
                            >
                                <Braces className="size-3.5" />
                                <span>Raw JSON</span>
                            </button>
                        </div>

                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleCopyJson}
                            className="gap-1.5 text-xs h-8 cursor-pointer"
                        >
                            {copiedJson ? <Check className="size-3.5 text-emerald-500" /> : <Copy className="size-3.5" />}
                            <span>{copiedJson ? "Copied" : "Copy"}</span>
                        </Button>
                    </div>
                )}
            </div>

            {/* Empty State */}
            {!metadata && (
                <div className="py-16 text-center space-y-4 border border-dashed rounded-xl bg-card/30">
                    <div className="size-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center mx-auto">
                        <FileJson className="size-6" />
                    </div>
                    <div className="space-y-1 max-w-md mx-auto">
                        <h3 className="text-base font-semibold text-foreground">
                            No Metadata on Version {currentVersion ? `v${currentVersion.versionNo}` : ""}
                        </h3>
                        <p className="text-xs text-muted-foreground">
                            This resource version does not have inspection metadata attached. Run a pipeline containing an inspector step to analyze metadata.
                        </p>
                    </div>
                </div>
            )}

            {/* Standard JsonTreeTable or Raw JSON */}
            {metadata && (
                <div>
                    {viewMode === "tree" ? (
                        <JsonTreeTable
                            data={metadata}
                            entityId={currentVersion?.id}
                            entityType="ResourceVersion"
                            tagsByPath={tagsByPath}
                        />
                    ) : (
                        <div className="rounded-xl border bg-card overflow-hidden">
                            <pre className="p-4 text-xs font-mono text-foreground overflow-auto max-h-[600px] leading-relaxed select-text bg-background/50">
                                {JSON.stringify(metadata, null, 2)}
                            </pre>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}
