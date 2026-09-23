import { useState, useMemo } from "react";
import { BaseDialog } from "@/components/custom-ui/overlays/dialog/BaseDialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
    Sparkles,
    Copy,
    Check,
    Search,
    Layers,
    Tag,
    FileCode,
    SlidersHorizontal,
} from "lucide-react";
import { simulateBuildTagMap, type TagMapPreviewResult } from "../../utils/simulateTagMap";
import type { TagLinkDetailDto } from "@/gen/model";
import { cn } from "@/lib/utils";

interface TagMapPreviewDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    resourceId?: string;
    versionId?: string;
    versionNo?: number;
    resourceName?: string;
    relativePath?: string;
    filePath?: string;
    metadata?: any;
    tagsByPath?: Record<string, TagLinkDetailDto[]>;
    resourceTags?: { tagPath: string; tagName?: string }[];
}

export function TagMapPreviewDialog({
    open,
    onOpenChange,
    resourceId = "",
    versionId = "",
    versionNo = 1,
    resourceName = "Resource",
    relativePath = "",
    filePath = "",
    metadata = null,
    tagsByPath = {},
    resourceTags = [],
}: TagMapPreviewDialogProps) {
    const [keyMode, setKeyMode] = useState<"Both" | "LeafOnly" | "FullPathOnly">("Both");
    const [activeTab, setActiveTab] = useState<"all" | "objects" | "path_map" | "tag_map" | "all_tags">("all");
    const [searchFilter, setSearchFilter] = useState("");
    const [copied, setCopied] = useState(false);

    // Compute preview
    const previewResult: TagMapPreviewResult = useMemo(() => {
        return simulateBuildTagMap({
            resourceId,
            versionId,
            resourceName,
            relativePath,
            filePath,
            metadata,
            tagsByPath,
            resourceTags,
            options: { keyMode },
        });
    }, [resourceId, versionId, resourceName, relativePath, filePath, metadata, tagsByPath, resourceTags, keyMode]);

    // Current output slice based on activeTab
    const currentDisplayData = useMemo(() => {
        const assetKey = Object.keys(previewResult.objects_map)[0] || resourceName;
        const asset = previewResult.objects_map[assetKey] || {};

        switch (activeTab) {
            case "objects":
                return previewResult.objects_map;
            case "path_map":
                return asset.path_map || {};
            case "tag_map":
                return asset.tag_map || {};
            case "all_tags":
                return previewResult.all_tags;
            case "all":
            default:
                return {
                    ObjectsMap: previewResult.objects_map,
                    AllTags: previewResult.all_tags,
                };
        }
    }, [activeTab, previewResult, resourceName]);

    // Format JSON with optional search highlight
    const jsonString = useMemo(() => {
        return JSON.stringify(currentDisplayData, null, 2);
    }, [currentDisplayData]);

    const handleCopy = () => {
        navigator.clipboard.writeText(jsonString);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };

    return (
        <BaseDialog
            open={open}
            onOpenChange={onOpenChange}
            size="2xl"
            title=""
            className="p-0 overflow-hidden max-w-4xl"
        >
            <div className="flex flex-col h-[750px] max-h-[85vh]">
                {/* Header */}
                <div className="p-4 border-b bg-muted/20 flex flex-col sm:flex-row sm:items-center justify-between gap-3 shrink-0">
                    <div className="flex items-center gap-2.5">
                        <div className="size-8 rounded-lg bg-amber-500/10 text-amber-500 flex items-center justify-center shrink-0">
                            <Sparkles className="size-4.5" />
                        </div>
                        <div>
                            <div className="flex items-center gap-2">
                                <h2 className="text-sm font-semibold text-foreground">
                                    BuildTagMap Output Preview
                                </h2>
                                <Badge variant="outline" className="font-mono text-[11px] font-bold">
                                    v{versionNo}
                                </Badge>
                            </div>
                            <p className="text-[11px] text-muted-foreground mt-0.5">
                                Simulated output of the <span className="font-mono text-foreground font-semibold">BuildTagMapFromResource</span> node.
                            </p>
                        </div>
                    </div>

                    {/* Quick Stats Badges */}
                    <div className="flex items-center gap-2 shrink-0">
                        <Badge variant="secondary" className="text-xs gap-1 font-normal py-1">
                            <Layers className="size-3 text-primary" />
                            <span>{previewResult.summary.mappedPathsCount} Mapped Paths</span>
                        </Badge>
                        <Badge variant="secondary" className="text-xs gap-1 font-normal py-1">
                            <Tag className="size-3 text-amber-500" />
                            <span>{previewResult.summary.uniqueTagsCount} Tags</span>
                        </Badge>
                    </div>
                </div>

                {/* Toolbar */}
                <div className="p-3 border-b bg-card flex flex-wrap items-center justify-between gap-2.5 shrink-0">
                    {/* View Tabs */}
                    <div className="flex items-center p-0.5 rounded-lg bg-muted border text-xs">
                        {[
                            { id: "all", label: "Full Output" },
                            { id: "tag_map", label: "Tag Map" },
                            { id: "path_map", label: "Path Map" },
                            { id: "objects", label: "ObjectsMap" },
                            { id: "all_tags", label: "All Tags" },
                        ].map((tab) => (
                            <button
                                key={tab.id}
                                type="button"
                                onClick={() => setActiveTab(tab.id as any)}
                                className={cn(
                                    "px-2.5 py-1 rounded-md font-medium transition-all cursor-pointer",
                                    activeTab === tab.id
                                        ? "bg-background text-foreground shadow-2xs font-semibold"
                                        : "text-muted-foreground hover:text-foreground"
                                )}
                            >
                                {tab.label}
                            </button>
                        ))}
                    </div>

                    {/* KeyMode Selector & Copy Button */}
                    <div className="flex items-center gap-2">
                        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                            <SlidersHorizontal className="size-3.5" />
                            <span>KeyMode:</span>
                            <div className="flex items-center p-0.5 rounded bg-muted border text-[11px]">
                                {(["Both", "LeafOnly", "FullPathOnly"] as const).map((mode) => (
                                    <button
                                        key={mode}
                                        type="button"
                                        onClick={() => setKeyMode(mode)}
                                        className={cn(
                                            "px-2 py-0.5 rounded cursor-pointer transition-colors",
                                            keyMode === mode
                                                ? "bg-background text-foreground font-semibold shadow-2xs"
                                                : "text-muted-foreground hover:text-foreground"
                                        )}
                                    >
                                        {mode}
                                    </button>
                                ))}
                            </div>
                        </div>

                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleCopy}
                            className="h-7 text-xs gap-1.5 cursor-pointer ml-1"
                        >
                            {copied ? <Check className="size-3 text-emerald-500" /> : <Copy className="size-3" />}
                            <span>{copied ? "Copied" : "Copy JSON"}</span>
                        </Button>
                    </div>
                </div>

                {/* Filter search bar */}
                <div className="px-3 py-2 border-b bg-muted/10 flex items-center gap-2 shrink-0">
                    <Search className="size-3.5 text-muted-foreground shrink-0" />
                    <Input
                        value={searchFilter}
                        onChange={(e) => setSearchFilter(e.target.value)}
                        placeholder="Search key, slot name, texture, or tag in preview..."
                        className="h-7 text-xs bg-background/50 border-border/60"
                    />
                    {searchFilter && (
                        <button
                            type="button"
                            onClick={() => setSearchFilter("")}
                            className="text-xs text-muted-foreground hover:text-foreground cursor-pointer px-1"
                        >
                            Clear
                        </button>
                    )}
                </div>

                {/* JSON Viewer */}
                <div className="flex-1 overflow-auto p-4 bg-muted/15 font-mono text-xs select-text">
                    {previewResult.summary.mappedPathsCount === 0 && previewResult.summary.uniqueTagsCount === 0 ? (
                        <div className="flex flex-col items-center justify-center h-full text-center p-6 text-muted-foreground">
                            <FileCode className="size-10 text-muted-foreground/30 mb-2" />
                            <p className="font-semibold text-foreground text-sm">No Tags Assigned to this Resource</p>
                            <p className="text-xs max-w-sm mt-1">
                                Assign tags to materials or slots in the Table View to preview the generated Tag Map here.
                            </p>
                        </div>
                    ) : (
                        <pre className="text-foreground leading-relaxed font-mono whitespace-pre-wrap break-all">
                            {jsonString}
                        </pre>
                    )}
                </div>

                {/* Footer Notes */}
                <div className="p-2.5 border-t bg-muted/20 flex items-center justify-between text-[11px] text-muted-foreground shrink-0">
                    <span>
                        Target: <span className="font-mono text-foreground">{resourceName}</span> (ID: <span className="font-mono">{resourceId.substring(0, 8)}...</span>)
                    </span>
                    <Button variant="ghost" size="sm" onClick={() => onOpenChange(false)} className="h-6 text-xs">
                        Close
                    </Button>
                </div>
            </div>
        </BaseDialog>
    );
}
