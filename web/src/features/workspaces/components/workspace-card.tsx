import type { WorkspaceDto } from "../hooks/useWorkspaces";
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Bot, FileText, MoreVertical, Edit3, Trash2, ArrowRight, Layers } from "lucide-react";
import { useDialogStore } from "@/stores/dialogStore";
import { usePlatforms } from "@/features/platforms/hooks/usePlatforms";
import { Link } from "@tanstack/react-router";
import { cn } from "@/lib/utils";
import {
  Menu,
  MenuItem,
  MenuTrigger,
  Popover,
} from "react-aria-components";

interface WorkspaceCardProps {
  workspace: WorkspaceDto;
}

export function WorkspaceCard({ workspace }: WorkspaceCardProps) {
  const openDialog = useDialogStore((state) => state.openDialog);
  const { data: platforms } = usePlatforms();

  const platformMap = new Map((platforms || []).map((p) => [p.id, p.name]));
  const platformIds = (workspace as any).platformIds as string[] | undefined;

  return (
    <Card className="group relative border bg-card hover:shadow-md transition-all duration-200 flex flex-col justify-between overflow-hidden">
      <div className="absolute top-0 left-0 w-1 h-full bg-primary/40 group-hover:bg-primary transition-colors" />

      <CardHeader className="pl-6 pr-4 pt-4 pb-2">
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-primary/10 text-primary">
              <Layers className="size-5" />
            </div>
            <div>
              <CardTitle className="text-lg font-semibold tracking-tight line-clamp-1 group-hover:text-primary transition-colors">
                {workspace.name}
              </CardTitle>
              <CardDescription className="text-xs text-muted-foreground mt-0.5">
                Created {new Date(workspace.createdAt).toLocaleDateString()}
              </CardDescription>
            </div>
          </div>

          <MenuTrigger>
            <Button variant="ghost" size="icon" className="size-8 text-muted-foreground hover:text-foreground">
              <MoreVertical className="size-4" />
            </Button>
            <Popover className="min-w-[140px] rounded-md border bg-popover p-1 shadow-md text-popover-foreground">
              <Menu className="outline-none">
                <MenuItem
                  onAction={() =>
                    openDialog("update-workspace", {
                      id: workspace.id,
                      name: workspace.name,
                      platformIds: platformIds,
                    })
                  }
                  className="flex items-center gap-2 px-2 py-1.5 text-xs rounded-sm cursor-pointer hover:bg-accent hover:text-accent-foreground outline-none"
                >
                  <Edit3 className="size-3.5" />
                  <span>Edit</span>
                </MenuItem>
                <MenuItem
                  onAction={() => openDialog("delete-workspace", { id: workspace.id, name: workspace.name })}
                  className="flex items-center gap-2 px-2 py-1.5 text-xs rounded-sm cursor-pointer text-destructive hover:bg-destructive/10 outline-none"
                >
                  <Trash2 className="size-3.5" />
                  <span>Delete</span>
                </MenuItem>
              </Menu>
            </Popover>
          </MenuTrigger>
        </div>
      </CardHeader>

      <CardContent className="px-6 py-3 space-y-2.5">
        {platformIds && platformIds.length > 0 && (
          <div className="flex flex-wrap gap-1.5">
            {platformIds.map((pId) => (
              <Badge key={pId} variant="secondary" className="text-[11px] px-2 py-0.5">
                {platformMap.get(pId) || "Platform"}
              </Badge>
            ))}
          </div>
        )}

        <div className="grid grid-cols-2 gap-3 pt-2 border-t border-border/50 text-xs">
          <div className="flex items-center gap-2 text-muted-foreground">
            <Bot className="size-4 text-primary/70" />
            <span>
              <strong className="text-foreground font-medium">{workspace.agentCount}</strong> Agents
            </span>
          </div>
          <div className="flex items-center gap-2 text-muted-foreground">
            <FileText className="size-4 text-primary/70" />
            <span>
              <strong className="text-foreground font-medium">{workspace.resourceCount}</strong> Resources
            </span>
          </div>
        </div>
      </CardContent>

      <CardFooter className="px-6 pb-4 pt-2">
        <Link
          to="/projects/$projectId/workspaces/$workspaceId"
          params={{ projectId: workspace.projectId, workspaceId: workspace.id }}
          className={cn(
            buttonVariants({ variant: "outline", size: "sm" }),
            "w-full justify-between group-hover:border-primary/50 cursor-pointer"
          )}
        >
          <span>Open Workspace</span>
          <ArrowRight className="size-3.5 transition-transform group-hover:translate-x-1" />
        </Link>
      </CardFooter>
    </Card>
  );
}
