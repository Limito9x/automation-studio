import { useMemo } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { BaseTable } from "@/components/table/BaseTable";
import { useDataTable } from "@/lib/useDataTable";
import type { WorkspaceAgentResourceDto } from "../types/workspace-resources";
import type { useResourceQuery, BaseSearchParams } from "@/lib/useResourceQuery";
import { 
  FileCode, 
  Search, 
  Tag, 
  Calendar, 
  GitBranch, 
  CheckCircle2,
  HardDrive
} from "lucide-react";
import { Input } from "@/components/ui/input";

interface WorkspaceAgentResourceTableProps {
  data: WorkspaceAgentResourceDto[];
  totalCount: number;
  isLoading: boolean;
  resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
}

export function WorkspaceAgentResourceTable({
  data,
  totalCount,
  isLoading,
  resource,
}: WorkspaceAgentResourceTableProps) {
  const columns = useMemo<ColumnDef<WorkspaceAgentResourceDto>[]>(
    () => [
      {
        accessorKey: "resourceName",
        header: "Resource Name",
        meta: { label: "Name", icon: FileCode },
        cell: ({ row }) => {
          const item = row.original;
          return (
            <div className="flex items-center gap-2.5 py-1 min-w-[180px]">
              <div className="size-8 rounded-lg bg-muted flex items-center justify-center text-muted-foreground shrink-0">
                <FileCode className="size-4 text-primary" />
              </div>
              <div className="min-w-0">
                <p className="font-semibold text-foreground text-sm truncate">{item.resourceName}</p>
                <p className="text-xs text-muted-foreground font-mono truncate max-w-xs">{item.relativePath}</p>
              </div>
            </div>
          );
        },
      },
      {
        accessorKey: "versionNo",
        header: "Version",
        meta: { label: "Version", icon: GitBranch },
        cell: ({ row }) => {
          const item = row.original;
          return (
            <div className="flex items-center gap-2">
              <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-muted text-xs font-mono font-medium text-foreground">
                <GitBranch className="size-3 text-muted-foreground" />
                v{item.versionNo}
              </span>
              {item.isOrigin && (
                <span className="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-primary/10 text-primary text-[10px] font-semibold border border-primary/20">
                  <CheckCircle2 className="size-2.5" /> Origin
                </span>
              )}
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
              className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium border"
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
        accessorKey: "fileHash",
        header: "Hash (Checksum)",
        meta: { label: "Checksum", icon: HardDrive },
        cell: ({ row }) => {
          const hash = row.original.fileHash;
          if (!hash) return <span className="text-xs text-muted-foreground">-</span>;
          return (
            <code className="text-[11px] bg-muted px-1.5 py-0.5 rounded font-mono text-muted-foreground truncate max-w-[120px] inline-block">
              {hash.slice(0, 12)}...
            </code>
          );
        },
      },
      {
        accessorKey: "discoveredAt",
        header: "Discovered At",
        meta: { label: "Discovered", icon: Calendar },
        cell: ({ row }) => {
          const date = row.original.discoveredAt ? new Date(row.original.discoveredAt) : null;
          return (
            <span className="text-xs text-muted-foreground">
              {date ? date.toLocaleString() : "N/A"}
            </span>
          );
        },
      },
    ],
    []
  );

  const table = useDataTable({
    data,
    columns,
    totalCount,
    resource,
  });

  return (
    <div className="space-y-4">
      {/* Omni-Search Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div className="relative w-full sm:w-80">
          <Search className="size-4 absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search agent files..."
            value={resource.search.globalKeyword || ""}
            onChange={(e) => resource.onSearchChange(e.target.value)}
            className="pl-9 text-xs"
          />
        </div>
        <div className="text-xs text-muted-foreground font-medium">
          Showing <span className="text-foreground font-semibold">{data.length}</span> of{" "}
          <span className="text-foreground font-semibold">{totalCount}</span> file locations
        </div>
      </div>

      {/* Table */}
      <BaseTable
        table={table}
        columns={columns}
        isLoading={isLoading}
        caption="Agent Resources List"
      />
    </div>
  );
}
