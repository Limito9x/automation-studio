import type { AgentDto } from "@/gen/model";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Server, Activity, Clock, Key, Cpu } from "lucide-react";
import { useTranslation } from "react-i18next";
import { formatDistanceToNow } from "date-fns";

interface AgentCardProps {
    agent: AgentDto;
    onConfigureExecutors?: (agent: AgentDto) => void;
}

export function AgentCard({ agent, onConfigureExecutors }: AgentCardProps) {
    const { t } = useTranslation();

    const isOnline = agent.isActive && agent.lastSeenAt 
        ? (new Date().getTime() - new Date(agent.lastSeenAt).getTime() < 5 * 60 * 1000) 
        : agent.isActive;

    return (
        <Card className="hover:shadow-md transition-all duration-200 border-border/60 flex flex-col justify-between">
            <div>
                <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
                    <CardTitle className="text-base font-semibold flex items-center gap-2 truncate">
                        <div className="p-2 bg-primary/10 rounded-lg text-primary">
                            <Server className="w-4 h-4" />
                        </div>
                        <span className="truncate">{agent.name}</span>
                    </CardTitle>
                    <Badge 
                        variant={isOnline ? "default" : "secondary"}
                        className={isOnline ? "bg-emerald-500/15 text-emerald-600 hover:bg-emerald-500/25 border-emerald-500/30" : "bg-muted text-muted-foreground"}
                    >
                        <Activity className="w-3 h-3 mr-1 animate-pulse" />
                        {isOnline ? t("common.online", { defaultValue: "Online" }) : t("common.offline", { defaultValue: "Offline" })}
                    </Badge>
                </CardHeader>
                <CardContent className="space-y-3 pt-2 text-xs">
                    <div className="flex items-center text-muted-foreground gap-1.5 font-mono bg-muted/40 p-2 rounded border">
                        <Key className="w-3.5 h-3.5 shrink-0 text-primary/70" />
                        <span className="truncate" title={agent.machineKey}>{agent.machineKey}</span>
                    </div>

                    <div className="flex items-center justify-between text-muted-foreground pt-1 border-t">
                        <div className="flex items-center gap-1">
                            <Clock className="w-3.5 h-3.5" />
                            <span>{t("agents.lastSeen", { defaultValue: "Last Seen:" })}</span>
                        </div>
                        <span className="font-medium text-foreground">
                            {agent.lastSeenAt 
                                ? formatDistanceToNow(new Date(agent.lastSeenAt), { addSuffix: true })
                                : t("common.never", { defaultValue: "Never" })}
                        </span>
                    </div>
                </CardContent>
            </div>

            <div className="p-4 pt-0 border-t mt-2 flex justify-end">
                <Button
                    variant="outline"
                    size="sm"
                    className="w-full text-xs flex items-center justify-center gap-1.5"
                    onPress={() => onConfigureExecutors?.(agent)}
                >
                    <Cpu className="w-3.5 h-3.5 text-primary" />
                    {t("agents.configureExecutors", { defaultValue: "Executors & Runtime" })}
                </Button>
            </div>
        </Card>
    );
}
