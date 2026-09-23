import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import type { ResourceDiffItem } from "@/gen/model";
import { 
  FilePlus, 
  FileEdit, 
  FileMinus, 
  Layers,
  ArrowUpCircle
} from "lucide-react";

export type DiffItemWithStatus = ResourceDiffItem & {
  status: "added" | "modified" | "deleted";
};

interface LocalChangesTableProps {
  items: DiffItemWithStatus[];
  selectedPaths: Set<string>;
  onToggleSelect: (path: string) => void;
  onToggleSelectAll: () => void;
  newResourceNames: Record<string, string>;
  onNameChange: (path: string, newName: string) => void;
}

export function formatFileSize(bytes?: number | null): string {
  if (bytes === undefined || bytes === null || bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
}

export function LocalChangesTable({
  items,
  selectedPaths,
  onToggleSelect,
  onToggleSelectAll,
  newResourceNames,
  onNameChange,
}: LocalChangesTableProps) {
  const isAllSelected = items.length > 0 && items.every((i) => selectedPaths.has(i.relativePath));
  const isSomeSelected = items.some((i) => selectedPaths.has(i.relativePath)) && !isAllSelected;

  return (
    <div className="w-full overflow-x-auto rounded-xl border bg-card/60 backdrop-blur-xs shadow-2xs">
      <table className="w-full text-left text-xs border-collapse">
        <thead className="border-b bg-muted/40 text-muted-foreground uppercase font-semibold tracking-wider text-[11px]">
          <tr>
            <th className="py-3 px-4 w-10 text-center">
              <Checkbox
                isSelected={isAllSelected}
                isIndeterminate={isSomeSelected}
                onChange={onToggleSelectAll}
                aria-label="Select all items"
              />
            </th>
            <th className="py-3 px-4 min-w-[240px]">File & Local Path</th>
            <th className="py-3 px-4 min-w-[130px]">Change Type</th>
            <th className="py-3 px-4 min-w-[100px]">Size</th>
            <th className="py-3 px-4 min-w-[280px]">Planned Action & Name</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-border/60">
          {items.length === 0 ? (
            <tr>
              <td colSpan={5} className="py-12 text-center text-muted-foreground">
                <Layers className="size-8 mx-auto mb-2 opacity-40" />
                <p className="text-sm font-medium">No local changes found</p>
                <p className="text-xs opacity-75">Your local files match the remote workspace state.</p>
              </td>
            </tr>
          ) : (
            items.map((item) => {
              const isSelected = selectedPaths.has(item.relativePath);
              const customName = newResourceNames[item.relativePath] ?? item.name;

              return (
                <tr
                  key={item.relativePath}
                  className={`transition-colors hover:bg-muted/30 ${
                    isSelected ? "bg-primary/5" : ""
                  }`}
                >
                  {/* Selection Checkbox */}
                  <td className="py-3 px-4 text-center">
                    <Checkbox
                      isSelected={isSelected}
                      onChange={() => onToggleSelect(item.relativePath)}
                      aria-label={`Select ${item.relativePath}`}
                    />
                  </td>

                  {/* File & Relative Path */}
                  <td className="py-3 px-4">
                    <div className="flex items-center gap-2.5">
                      <div className="size-8 rounded-lg bg-muted flex items-center justify-center shrink-0">
                        {item.status === "added" && <FilePlus className="size-4 text-emerald-500" />}
                        {item.status === "modified" && <FileEdit className="size-4 text-amber-500" />}
                        {item.status === "deleted" && <FileMinus className="size-4 text-destructive" />}
                      </div>
                      <div className="min-w-0 max-w-sm">
                        <p className="font-semibold text-foreground truncate">{item.name}</p>
                        <p className="text-[11px] text-muted-foreground font-mono truncate">
                          {item.relativePath}
                        </p>
                      </div>
                    </div>
                  </td>

                  {/* Status Badge */}
                  <td className="py-3 px-4">
                    {item.status === "added" && (
                      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 border border-emerald-500/30">
                        <FilePlus className="size-3" />
                        Added (New)
                      </span>
                    )}
                    {item.status === "modified" && (
                      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-amber-500/15 text-amber-600 dark:text-amber-400 border border-amber-500/30">
                        <FileEdit className="size-3" />
                        Modified
                      </span>
                    )}
                    {item.status === "deleted" && (
                      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-destructive/15 text-destructive border border-destructive/30">
                        <FileMinus className="size-3" />
                        Deleted
                      </span>
                    )}
                  </td>

                  {/* Size */}
                  <td className="py-3 px-4 text-muted-foreground font-mono">
                    {item.status === "deleted"
                      ? formatFileSize(item.remoteVersion?.sizeBytes)
                      : formatFileSize(item.localFileSize)}
                  </td>

                  {/* Action & Custom Name */}
                  <td className="py-3 px-4">
                    {item.status === "added" && (
                      <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                        <span className="text-xs text-emerald-600 dark:text-emerald-400 font-medium whitespace-nowrap">
                          Create:
                        </span>
                        <Input
                          value={customName}
                          onChange={(e) => onNameChange(item.relativePath, e.target.value)}
                          placeholder="Resource Name"
                          className="h-8 text-xs max-w-xs bg-background"
                        />
                      </div>
                    )}

                    {item.status === "modified" && (
                      <div className="flex items-center gap-2 text-xs text-amber-600 dark:text-amber-400 font-medium">
                        <ArrowUpCircle className="size-3.5 shrink-0" />
                        <span>
                          Bump to v{(item.remoteVersion?.versionNo ?? 0) + 1}{" "}
                          <span className="text-muted-foreground font-normal">
                            (Current: v{item.remoteVersion?.versionNo ?? 1})
                          </span>
                        </span>
                      </div>
                    )}

                    {item.status === "deleted" && (
                      <div className="flex items-center gap-2 text-xs text-destructive font-medium">
                        <FileMinus className="size-3.5 shrink-0" />
                        <span>Unlink from Agent</span>
                      </div>
                    )}
                  </td>
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}
