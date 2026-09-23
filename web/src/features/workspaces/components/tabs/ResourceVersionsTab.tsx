import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import {
    History,
    CheckCircle2,
    Laptop,
    HardDrive,
    Cloud,
    Copy,
    Check,
    Calendar,
    Sparkles,
} from "lucide-react";
import type { ResourceVersionDto } from "@/gen/model";

interface ResourceVersionsTabProps {
    versions: ResourceVersionDto[];
    resourceName?: string;
    filePath?: string;
}

export function ResourceVersionsTab({ versions, resourceName, filePath }: ResourceVersionsTabProps) {
    const [copiedHash, setCopiedHash] = useState<string | null>(null);

    const handleCopyHash = (hash: string) => {
        navigator.clipboard.writeText(hash);
        setCopiedHash(hash);
        setTimeout(() => setCopiedHash(null), 2000);
    };

    const formatBytes = (bytes: number, decimals = 2) => {
        if (!+bytes) return "0 Bytes";
        const k = 1024;
        const dm = decimals < 0 ? 0 : decimals;
        const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
    };

    return (
        <div className="space-y-4">
            {/* Tab Header Banner */}
            <div className="p-3.5 bg-muted/20 border border-border/70 rounded-xl flex items-center justify-between gap-3 shadow-xs">
                <div className="flex items-center gap-2.5 min-w-0">
                    <div className="w-8 h-8 rounded-lg bg-blue-500/10 border border-blue-500/20 flex items-center justify-center text-blue-400 shrink-0">
                        <History className="w-4 h-4" />
                    </div>
                    <div>
                        <div className="text-xs font-semibold text-foreground">
                            Resource Version History
                        </div>
                        <div className="text-xs text-muted-foreground">
                            Track commit origin, pushing agent workstation, and network replica locations.
                        </div>
                    </div>
                </div>
                <Badge variant="outline" className="font-mono text-xs py-1 px-2.5">
                    {versions.length} Versions
                </Badge>
            </div>

            {/* Versions Cards List */}
            {versions.length === 0 ? (
                <div className="py-12 text-center text-xs text-muted-foreground border border-dashed rounded-xl bg-card/40 space-y-2">
                    <History className="w-8 h-8 text-muted-foreground/40 mx-auto" />
                    <p className="font-medium text-sm text-foreground">No recorded versions found.</p>
                </div>
            ) : (
                <div className="space-y-4">
                    {versions.map((version, index) => {
                        const isLatest = index === 0;
                        const createdDateStr = version.createdAt
                            ? new Date(version.createdAt).toLocaleString()
                            : "N/A";

                        return (
                            <div
                                key={version.id}
                                className={`border rounded-xl bg-card overflow-hidden shadow-xs transition-all ${
                                    isLatest ? "border-primary/40 ring-1 ring-primary/20" : "border-border/70"
                                }`}
                            >
                                {/* Version Header Bar */}
                                <div className="p-4 bg-muted/20 border-b flex flex-wrap items-center justify-between gap-3">
                                    <div className="flex items-center gap-2.5 flex-wrap">
                                        <Badge
                                            className={`text-xs font-mono font-semibold py-0.5 px-2.5 ${
                                                isLatest
                                                    ? "bg-primary text-primary-foreground"
                                                    : "bg-secondary text-secondary-foreground"
                                            }`}
                                        >
                                            Version V{version.versionNo}
                                        </Badge>

                                        {isLatest && (
                                            <Badge
                                                variant="outline"
                                                className="bg-emerald-500/10 text-emerald-400 border-emerald-500/30 gap-1 py-0.5 px-2"
                                            >
                                                <Sparkles className="w-3 h-3" />
                                                <span>Current Version</span>
                                            </Badge>
                                        )}

                                        <span className="font-semibold text-sm text-foreground">
                                            v{version.versionNo}.0.0
                                        </span>
                                    </div>

                                    <div className="flex items-center gap-3 text-xs text-muted-foreground">
                                        <span className="flex items-center gap-1.5 font-mono">
                                            <Calendar className="w-3.5 h-3.5" />
                                            {createdDateStr}
                                        </span>
                                    </div>
                                </div>

                                {/* Version Notes / Description */}
                                {version.notes && (
                                    <div className="p-3.5 mx-4 mt-4 text-xs bg-accent/20 border border-border/40 rounded-lg text-foreground space-y-1">
                                        <div className="font-semibold text-[11px] uppercase tracking-wider text-muted-foreground">
                                            Release Notes & Changes:
                                        </div>
                                        <p className="leading-relaxed">{version.notes}</p>
                                    </div>
                                )}

                                {/* 2 Columns Grid: Origin vs Synced Nodes */}
                                <div className="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                                    {/* Column 1: Origin Source Machine */}
                                    <div className="p-3.5 rounded-lg border border-border/50 bg-background/50 space-y-2.5">
                                        <div className="flex items-center justify-between pb-2 border-b border-border/40 text-xs">
                                            <span className="font-semibold flex items-center gap-1.5 text-foreground">
                                                <Laptop className="w-4 h-4 text-blue-400" />
                                                Pushed from Local Agent
                                            </span>
                                            <Badge variant="outline" className="bg-blue-500/10 text-blue-400 border-blue-500/20 text-[10px]">
                                                Origin Source
                                            </Badge>
                                        </div>

                                        <div className="space-y-1.5 text-xs">
                                            <div className="flex items-center justify-between text-muted-foreground">
                                                <span>Workstation Agent:</span>
                                                <span className="font-medium text-foreground">Local Host Machine</span>
                                            </div>
                                            <div className="flex items-center justify-between text-muted-foreground">
                                                <span>Operating System:</span>
                                                <span className="font-medium text-foreground">Windows / Linux</span>
                                            </div>
                                            <div className="pt-1.5 space-y-1">
                                                <span className="text-muted-foreground text-[11px] block">
                                                    Local Source File Path:
                                                </span>
                                                <div className="p-2 bg-muted/40 rounded border border-border/40 font-mono text-[11px] text-primary truncate">
                                                    {filePath || resourceName || "N/A"}
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Column 2: Synchronized Nodes & Cloud Vault */}
                                    <div className="p-3.5 rounded-lg border border-border/50 bg-background/50 space-y-2.5">
                                        <div className="flex items-center justify-between pb-2 border-b border-border/40 text-xs">
                                            <span className="font-semibold flex items-center gap-1.5 text-foreground">
                                                <HardDrive className="w-4 h-4 text-emerald-400" />
                                                Network Synchronization Locations
                                            </span>
                                            <Badge variant="outline" className="bg-emerald-500/10 text-emerald-400 border-emerald-500/20 text-[10px]">
                                                100% Synced
                                            </Badge>
                                        </div>

                                        <div className="space-y-2 text-xs">
                                            {/* Synced Node 1 */}
                                            <div className="p-2 bg-muted/30 rounded border border-border/40 flex items-center justify-between gap-2">
                                                <div className="min-w-0">
                                                    <div className="font-medium text-foreground truncate flex items-center gap-1.5">
                                                        <span>Local Workspace Repository</span>
                                                    </div>
                                                    <div className="font-mono text-[10px] text-muted-foreground truncate">
                                                        {filePath || "Workspace Directory"}
                                                    </div>
                                                </div>
                                                <Badge variant="outline" className="bg-emerald-500/10 text-emerald-400 border-emerald-500/20 text-[10px] shrink-0 gap-1">
                                                    <CheckCircle2 className="w-3 h-3" />
                                                    <span>Synced</span>
                                                </Badge>
                                            </div>

                                            {/* Synced Node 2: Cloud S3 Master */}
                                            <div className="p-2 bg-muted/30 rounded border border-border/40 flex items-center justify-between gap-2">
                                                <div className="min-w-0">
                                                    <div className="font-medium text-foreground truncate flex items-center gap-1.5">
                                                        <Cloud className="w-3.5 h-3.5 text-sky-400" />
                                                        <span>Cloud S3 Master Vault</span>
                                                    </div>
                                                    <div className="font-mono text-[10px] text-muted-foreground truncate">
                                                        s3://asset-vault/resources/{version.fileHash.slice(0, 16)}
                                                    </div>
                                                </div>
                                                <Badge variant="outline" className="bg-emerald-500/10 text-emerald-400 border-emerald-500/20 text-[10px] shrink-0 gap-1">
                                                    <CheckCircle2 className="w-3 h-3" />
                                                    <span>Synced</span>
                                                </Badge>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                {/* Footer Card: SHA-256 and Size */}
                                <div className="p-3 bg-muted/10 border-t flex flex-wrap items-center justify-between gap-3 text-xs">
                                    <div className="flex items-center gap-2 min-w-0">
                                        <span className="text-muted-foreground font-mono text-[11px] shrink-0">
                                            SHA-256 Checksum:
                                        </span>
                                        <div className="flex items-center gap-1 bg-background/80 px-2 py-1 rounded border font-mono text-[11px] text-foreground">
                                            <span className="truncate max-w-[200px] sm:max-w-[320px]">
                                                {version.fileHash}
                                            </span>
                                            <button
                                                type="button"
                                                onClick={() => handleCopyHash(version.fileHash)}
                                                className="hover:text-primary transition-colors ml-1 p-0.5 cursor-pointer"
                                                title="Copy Checksum"
                                            >
                                                {copiedHash === version.fileHash ? (
                                                    <Check className="w-3 h-3 text-emerald-400" />
                                                ) : (
                                                    <Copy className="w-3 h-3" />
                                                )}
                                            </button>
                                        </div>
                                    </div>

                                    <div className="text-muted-foreground text-xs font-mono">
                                        File Size:{" "}
                                        <span className="font-semibold text-foreground">
                                            {formatBytes(version.sizeBytes)}
                                        </span>{" "}
                                        ({version.sizeBytes.toLocaleString()} bytes)
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
}
