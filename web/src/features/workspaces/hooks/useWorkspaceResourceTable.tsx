import { useMemo, useState } from "react";
import {
  type ColumnDef,
  useReactTable,
  getCoreRowModel,
  type RowSelectionState,
} from "@tanstack/react-table";
import { Link } from "@tanstack/react-router";
import type { WorkspaceResourceDto } from "../types/workspace-resources";
import type { useResourceQuery, BaseSearchParams } from "@/lib/useResourceQuery";
import {
  FileCode,
  Tag,
  Calendar,
  GitBranch,
  Folder,
  Unlink,
  Sparkles,
  Check,
  Minus,
  Eye,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { DraggableResourceRowHandle } from "../components/DraggableResourceRowHandle";
import { useAssignResourcesContent } from "./useWorkspaceResources";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

export interface UseWorkspaceResourceTableOptions {
  data: WorkspaceResourceDto[];
  totalCount: number;
  resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
  workspaceId: string;
  projectId?: string;
  onOpenAssignPanel: (resourceId?: string) => void;
}

function TableCheckbox({
  isSelected,
  isIndeterminate,
  onChange,
  ariaLabel,
}: {
  isSelected?: boolean;
  isIndeterminate?: boolean;
  onChange: (checked: boolean) => void;
  ariaLabel?: string;
}) {
  return (
    <button
      type="button"
      role="checkbox"
      aria-checked={isIndeterminate ? "mixed" : isSelected}
      aria-label={ariaLabel}
      onClick={(e) => {
        e.stopPropagation();
        onChange(!isSelected);
      }}
      className={cn(
        "relative flex size-4 shrink-0 items-center justify-center rounded-[4px] border border-input transition-all outline-none cursor-pointer",
        "focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30",
        isSelected || isIndeterminate
          ? "bg-primary text-primary-foreground border-primary"
          : "hover:border-primary/60 bg-background"
      )}
    >
      {isIndeterminate ? (
        <Minus className="size-3 text-current stroke-[3]" />
      ) : isSelected ? (
        <Check className="size-3 text-current stroke-[3]" />
      ) : null}
    </button>
  );
}

export function useWorkspaceResourceTable({
  data,
  totalCount,
  resource,
  workspaceId,
  projectId,
  onOpenAssignPanel,
}: UseWorkspaceResourceTableOptions) {
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

  const assignMutation = useAssignResourcesContent(workspaceId);

  const selectedRowIds = useMemo(() => {
    return Object.keys(rowSelection).filter((id) => rowSelection[id]);
  }, [rowSelection]);

  const handleUnlink = (resourceId: string, resourceName: string) => {
    assignMutation.mutate(
      {
        data: {
          resourceIds: [resourceId],
          contentId: null,
        },
      },
      {
        onSuccess: () => {
          toast.success(`Unlinked "${resourceName}" from content.`);
        },
        onError: (err: any) => {
          toast.error(err?.message || "Failed to unlink resource.");
        },
      }
    );
  };

  const handleBatchUnlink = () => {
    if (selectedRowIds.length === 0) return;
    assignMutation.mutate(
      {
        data: {
          resourceIds: selectedRowIds,
          contentId: null,
        },
      },
      {
        onSuccess: () => {
          toast.success(`Unlinked ${selectedRowIds.length} resource(s).`);
          setRowSelection({});
        },
        onError: (err: any) => {
          toast.error(err?.message || "Failed to unlink resources.");
        },
      }
    );
  };

  const columns = useMemo<ColumnDef<WorkspaceResourceDto>[]>(
    () => [
      {
        id: "select",
        header: ({ table }) => (
          <div className="flex items-center justify-center pl-1">
            <TableCheckbox
              isSelected={table.getIsAllPageRowsSelected()}
              isIndeterminate={table.getIsSomePageRowsSelected()}
              onChange={(checked) => table.toggleAllPageRowsSelected(checked)}
              ariaLabel="Select all resources"
            />
          </div>
        ),
        cell: ({ row }) => (
          <div className="flex items-center justify-center pl-1">
            <TableCheckbox
              isSelected={row.getIsSelected()}
              onChange={(checked) => row.toggleSelected(checked)}
              ariaLabel={`Select resource ${row.original.displayName || row.original.id}`}
            />
          </div>
        ),
        enableSorting: false,
        enableHiding: false,
        size: 38,
      },
      {
        id: "dragHandle",
        header: () => null,
        cell: ({ row }) => (
          <div className="flex items-center justify-center">
            <DraggableResourceRowHandle
              resource={row.original}
              selectedIds={selectedRowIds}
            />
          </div>
        ),
        enableSorting: false,
        enableHiding: false,
        size: 40,
      },
      {
        accessorKey: "name",
        header: "Resource File",
        meta: { label: "Name", icon: FileCode },
        cell: ({ row }) => {
          const item = row.original;

          return (
            <div className="flex items-center gap-2.5 py-1 min-w-[200px]">
              <div className="size-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary shrink-0 border border-primary/20">
                <FileCode className="size-4" />
              </div>
              <div className="min-w-0">
                <Link
                  to="/projects/$projectId/resources/$resourceId"
                  params={{ projectId: projectId || "", resourceId: item.id }}
                  search={{ workspaceId }}
                  className="font-semibold text-foreground hover:text-primary transition-colors text-sm truncate block group"
                >
                  <span className="group-hover:underline">{item.displayName || "Unnamed Resource"}</span>
                </Link>
                {item.relativePath && (
                  <p className="text-xs text-muted-foreground truncate max-w-xs font-mono text-[11px]">
                    {item.relativePath}
                  </p>
                )}
              </div>
            </div>
          );
        },
      },
      {
        accessorKey: "contentTypeName",
        header: "Content Type",
        meta: { label: "Type", icon: Tag },
        cell: ({ row }) => {
          const item = row.original;
          if (!item.contentTypeName) {
            return <span className="text-xs text-muted-foreground italic">Unassigned</span>;
          }
          return (
            <span
              className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border"
              style={{
                backgroundColor: item.contentTypeColor ? `${item.contentTypeColor}15` : undefined,
                color: item.contentTypeColor || undefined,
                borderColor: item.contentTypeColor ? `${item.contentTypeColor}40` : undefined,
              }}
            >
              <span
                className="size-1.5 rounded-full"
                style={{ backgroundColor: item.contentTypeColor || "#64748b" }}
              />
              {item.contentTypeName}
            </span>
          );
        },
      },
      {
        accessorKey: "contentName",
        header: "Linked Content",
        meta: { label: "Content", icon: Folder },
        cell: ({ row }) => {
          const item = row.original;
          if (!item.contentName) {
            return (
              <Button
                size="sm"
                variant="ghost"
                onClick={() => {
                  setRowSelection({ [item.id]: true });
                  onOpenAssignPanel(item.id);
                }}
                className="h-6 px-2 text-xs text-muted-foreground hover:text-primary gap-1"
              >
                <Sparkles className="size-3" />
                Assign
              </Button>
            );
          }
          return (
            <div className="flex items-center justify-between gap-2 max-w-[200px]">
              <div className="flex items-center gap-1.5 font-medium text-xs text-foreground truncate">
                <Folder className="size-3.5 text-muted-foreground shrink-0" />
                <span className="truncate">{item.contentName}</span>
              </div>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => handleUnlink(item.id, item.displayName || "")}
                className="size-6 p-0 text-muted-foreground hover:text-destructive shrink-0"
              >
                <Unlink className="size-3" />
              </Button>
            </div>
          );
        },
      },
      {
        accessorKey: "versionCount",
        header: "Versions",
        meta: { label: "Versions", icon: GitBranch },
        cell: ({ row }) => {
          const item = row.original;
          const count = item.versionCount;

          return (
            <Link
              to="/projects/$projectId/resources/$resourceId"
              params={{ projectId: projectId || "", resourceId: item.id }}
              search={{ workspaceId }}
              className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-muted hover:bg-primary/15 hover:text-primary text-xs font-mono font-medium text-foreground transition-colors"
            >
              <GitBranch className="size-3 text-muted-foreground" />
              v{count > 0 ? count : 1}
            </Link>
          );
        },
      },
      {
        id: "actions",
        header: "Detail & Metadata",
        meta: { label: "Detail", icon: Eye },
        cell: ({ row }) => {
          const item = row.original;

          return (
            <Link
              to="/projects/$projectId/resources/$resourceId"
              params={{ projectId: projectId || "", resourceId: item.id }}
              search={{ workspaceId }}
              className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-medium bg-primary/10 hover:bg-primary text-primary hover:text-primary-foreground transition-all"
            >
              <Eye className="size-3.5" />
              <span>Detail & Metadata</span>
            </Link>
          );
        },
      },
      {
        accessorKey: "createdAt",
        header: "Created At",
        meta: { label: "Created", icon: Calendar },
        cell: ({ row }) => {
          const date = row.original.createdAt ? new Date(row.original.createdAt) : null;
          return (
            <span className="text-xs text-muted-foreground">
              {date ? date.toLocaleDateString() : "N/A"}
            </span>
          );
        },
      },
    ],
    [selectedRowIds, onOpenAssignPanel, projectId, workspaceId]
  );

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    manualSorting: true,
    rowCount: totalCount,
    getRowId: (row) => row.id,
    state: {
      sorting: resource.sorting,
      pagination: resource.pagination,
      rowSelection,
    },
    onRowSelectionChange: setRowSelection,
    onSortingChange: resource.onSortingChange,
    onPaginationChange: resource.onPaginationChange,
  });

  return {
    table,
    columns,
    selectedRowIds,
    setRowSelection,
    handleUnlink,
    handleBatchUnlink,
    assignMutation,
  };
}
