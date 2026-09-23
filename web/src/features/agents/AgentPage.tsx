import { useState } from "react";
import type { AgentDto } from "@/gen/model";
import { useAgents } from "./hooks/useAgents";
import { AgentCard } from "./components/AgentCard";
import { GenerateSetupTokenDialog } from "./dialogs/GenerateSetupTokenDialog";
import { AgentExecutorsDialog } from "./dialogs/AgentExecutorsDialog";
import { Button } from "@/components/ui/button";
import { Plus, Server, RefreshCw } from "lucide-react";
import { useTranslation } from "react-i18next";

export function AgentPage() {
    const { t } = useTranslation();
    const { data: agentsData, isLoading, refetch } = useAgents();
    const [setupDialogOpen, setSetupDialogOpen] = useState(false);
    const [executorsAgent, setExecutorsAgent] = useState<AgentDto | null>(null);

    const agents = Array.isArray(agentsData) ? agentsData : [];

    return (
        <div className="container mx-auto p-6 space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b pb-5">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
                        <Server className="w-6 h-6 text-primary" />
                        {t("agents.title", { defaultValue: "Agent Management" })}
                    </h1>
                    <p className="text-sm text-muted-foreground mt-1">
                        {t("agents.subtitle", { defaultValue: "Manage remote execution agents and cluster nodes." })}
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Button variant="outline" size="sm" onPress={() => refetch()}>
                        <RefreshCw className="w-4 h-4 mr-2" />
                        {t("common.refresh", { defaultValue: "Refresh" })}
                    </Button>
                    <Button onPress={() => setSetupDialogOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" />
                        {t("agents.addAgent", { defaultValue: "Add Agent" })}
                    </Button>
                </div>
            </div>

            {/* Content */}
            {isLoading ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {[1, 2, 3].map((i) => (
                        <div key={i} className="h-44 bg-muted/40 animate-pulse rounded-lg border" />
                    ))}
                </div>
            ) : agents.length === 0 ? (
                <div className="flex flex-col items-center justify-center p-12 text-center border border-dashed rounded-lg bg-card space-y-4">
                    <div className="p-4 bg-primary/10 rounded-full text-primary">
                        <Server className="w-10 h-10" />
                    </div>
                    <div className="space-y-1">
                        <h3 className="text-lg font-semibold">
                            {t("agents.noAgentsTitle", { defaultValue: "No Agents Connected" })}
                        </h3>
                        <p className="text-sm text-muted-foreground max-w-sm">
                            {t("agents.noAgentsDesc", { defaultValue: "Click 'Add Agent' to generate a setup token and register your first machine." })}
                        </p>
                    </div>
                    <Button onPress={() => setSetupDialogOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" />
                        {t("agents.addAgent", { defaultValue: "Add Agent" })}
                    </Button>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {agents.map((agent: any) => (
                        <AgentCard
                            key={agent.id}
                            agent={agent}
                            onConfigureExecutors={(a) => setExecutorsAgent(a)}
                        />
                    ))}
                </div>
            )}

            {/* Setup Token Dialog */}
            <GenerateSetupTokenDialog
                open={setupDialogOpen}
                onOpenChange={setSetupDialogOpen}
            />

            {/* Executor Config & Scan Dialog */}
            <AgentExecutorsDialog
                open={!!executorsAgent}
                onOpenChange={(open) => {
                    if (!open) setExecutorsAgent(null);
                }}
                agent={executorsAgent}
            />
        </div>
    );
}
