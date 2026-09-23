import { formatFileSize } from "./LocalChangesTable";
import type { ResourceDiffItem } from "@/gen/model";
import { DownloadCloud, GitBranch, FolderDown } from "lucide-react";

interface MissingResourcesTableProps {
  items: ResourceDiffItem[];
}

export function MissingResourcesTable({ items }: MissingResourcesTableProps) {
  return (
    <div className="w-full overflow-x-auto rounded-xl border bg-card/60 backdrop-blur-xs shadow-2xs">
      <table className="w-full text-left text-xs border-collapse">
        <thead className="border-b bg-muted/40 text-muted-foreground uppercase font-semibold tracking-wider text-[11px]">
          <tr>
            <th className="py-3 px-4 min-w-[240px]">File & Remote Path</th>
            <th className="py-3 px-4 min-w-[130px]">Remote Version</th>
            <th className="py-3 px-4 min-w-[100px]">Remote Size</th>
            <th className="py-3 px-4 min-w-[200px]">Action Status</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-border/60">
          {items.length === 0 ? (
            <tr>
              <td colSpan={4} className="py-12 text-center text-muted-foreground">
                <FolderDown className="size-8 mx-auto mb-2 opacity-40" />
                <p className="text-sm font-medium">No missing remote resources</p>
                <p className="text-xs opacity-75">All workspace resources exist on this agent.</p>
              </td>
            </tr>
          ) : (
            items.map((item) => (
              <tr key={item.relativePath} className="transition-colors hover:bg-muted/30">
                {/* File & Remote Path */}
                <td className="py-3 px-4">
                  <div className="flex items-center gap-2.5">
                    <div className="size-8 rounded-lg bg-sky-500/10 text-sky-500 flex items-center justify-center shrink-0">
                      <DownloadCloud className="size-4" />
                    </div>
                    <div className="min-w-0 max-w-sm">
                      <p className="font-semibold text-foreground truncate">{item.name}</p>
                      <p className="text-[11px] text-muted-foreground font-mono truncate">
                        {item.relativePath}
                      </p>
                    </div>
                  </div>
                </td>

                {/* Remote Version */}
                <td className="py-3 px-4">
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-muted text-xs font-mono font-medium text-foreground">
                    <GitBranch className="size-3 text-muted-foreground" />
                    v{item.remoteVersion?.versionNo ?? 1}
                  </span>
                </td>

                {/* Remote Size */}
                <td className="py-3 px-4 text-muted-foreground font-mono">
                  {formatFileSize(item.remoteVersion?.sizeBytes)}
                </td>

                {/* Action Status */}
                <td className="py-3 px-4">
                  <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-muted text-muted-foreground border">
                    <DownloadCloud className="size-3" />
                    Pull from Remote (Coming Soon)
                  </span>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
