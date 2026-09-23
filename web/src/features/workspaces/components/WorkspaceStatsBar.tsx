import { Bot, Layers, HardDrive } from "lucide-react";

interface WorkspaceStatsBarProps {
  agentCount: number;
  resourceCount: number;
  locationCount?: number;
}

export function WorkspaceStatsBar({
  agentCount,
  resourceCount,
  locationCount = 0,
}: WorkspaceStatsBarProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
      {/* Agents Stat */}
      <div className="p-4 rounded-xl border bg-card/60 backdrop-blur-xs shadow-2xs flex items-center gap-3">
        <div className="size-10 rounded-lg bg-primary/10 flex items-center justify-center text-primary shrink-0">
          <Bot className="size-5" />
        </div>
        <div className="min-w-0">
          <p className="text-xs text-muted-foreground font-medium">Connected Agents</p>
          <p className="text-xl font-bold text-foreground mt-0.5">{agentCount}</p>
        </div>
      </div>

      {/* Resources Stat */}
      <div className="p-4 rounded-xl border bg-card/60 backdrop-blur-xs shadow-2xs flex items-center gap-3">
        <div className="size-10 rounded-lg bg-amber-500/10 flex items-center justify-center text-amber-500 shrink-0">
          <Layers className="size-5" />
        </div>
        <div className="min-w-0">
          <p className="text-xs text-muted-foreground font-medium">Total Resources</p>
          <p className="text-xl font-bold text-foreground mt-0.5">{resourceCount}</p>
        </div>
      </div>

      {/* Locations / Versions Stat */}
      <div className="p-4 rounded-xl border bg-card/60 backdrop-blur-xs shadow-2xs flex items-center gap-3">
        <div className="size-10 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-500 shrink-0">
          <HardDrive className="size-5" />
        </div>
        <div className="min-w-0">
          <p className="text-xs text-muted-foreground font-medium">Sync Locations</p>
          <p className="text-xl font-bold text-foreground mt-0.5">
            {locationCount > 0 ? locationCount : resourceCount}
          </p>
        </div>
      </div>
    </div>
  );
}
