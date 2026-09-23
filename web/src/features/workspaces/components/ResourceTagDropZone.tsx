import { useMemo } from "react";
import { useDroppable } from "@dnd-kit/core";
import { Tag as TagIcon, X, Plus } from "lucide-react";
import { useTagLinks, useDeleteTagLink } from "@/features/tags/hooks/useTags";
import type { TagDropZonePayload, TagLinkDetailDto } from "@/features/tags/types";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

interface ResourceTagDropZoneProps {
    resourceId: string;
    projectId: string;
    className?: string;
}

export function ResourceTagDropZone({
    resourceId,
    projectId: _projectId,
    className,
}: ResourceTagDropZoneProps) {
    const { data: tagLinks = [] } = useTagLinks({
        entityType: "Resource",
        entityId: resourceId,
    });
    const deleteTagLink = useDeleteTagLink();

    const payload: TagDropZonePayload = {
        type: "tag-drop-zone",
        path: "",
        entityId: resourceId,
        entityType: "Resource",
    };

    const { setNodeRef, isOver } = useDroppable({
        id: `drop-resource-${resourceId}`,
        data: payload,
    });

    // Only display root Resource-level tags (where targetSubPath is empty or null)
    // Sub-path tags (textures, materials, slots) are rendered inside their respective JsonTreeTable cells.
    const tags = useMemo(() => {
        return ((tagLinks as TagLinkDetailDto[]) || []).filter(
            (tag) => !tag.targetSubPath || tag.targetSubPath.trim() === ""
        );
    }, [tagLinks]);

    return (
        <div
            ref={setNodeRef}
            className={cn(
                "inline-flex items-center flex-wrap gap-1.5 px-2.5 py-1 rounded-lg border border-dashed transition-all duration-200 min-h-[32px]",
                isOver
                    ? "ring-2 ring-primary border-primary bg-primary/10 shadow-sm"
                    : "border-border/60 hover:border-border bg-muted/30",
                className
            )}
        >
            <div className="flex items-center gap-1 text-[11px] font-medium text-muted-foreground mr-0.5 select-none">
                <TagIcon className="size-3 text-muted-foreground/80" />
                <span>Resource Tags:</span>
            </div>

            {tags.length > 0 ? (
                tags.map((tag) => {
                    const tagColor = tag.tagColor || "#3b82f6";
                    return (
                        <span
                            key={tag.tagLinkId}
                            className="group inline-flex items-center gap-1.5 pl-2 pr-1 py-0.5 rounded-md text-xs font-semibold border shadow-2xs select-none transition-all"
                            style={{
                                backgroundColor: `${tagColor}15`,
                                color: tagColor,
                                borderColor: `${tagColor}35`,
                            }}
                            title={tag.tagPath || tag.tagName}
                        >
                            <span>{tag.tagName}</span>
                            <button
                                type="button"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    e.preventDefault();
                                    deleteTagLink.mutate(
                                        { id: tag.tagLinkId },
                                        {
                                            onSuccess: () => toast.success("Tag removed"),
                                            onError: (err: any) =>
                                                toast.error(err?.message || "Failed to remove tag"),
                                        }
                                    );
                                }}
                                className="opacity-60 hover:opacity-100 hover:bg-black/20 dark:hover:bg-white/20 rounded p-0.5 transition-opacity cursor-pointer"
                                title="Remove tag"
                                aria-label="Remove tag"
                            >
                                <X className="size-3" />
                            </button>
                        </span>
                    );
                })
            ) : (
                <span className="inline-flex items-center gap-1 text-[11px] text-muted-foreground/70 italic select-none">
                    <Plus className="size-2.5" />
                    <span>Drop tag here (e.g. ClothType.Top)</span>
                </span>
            )}
        </div>
    );
}
