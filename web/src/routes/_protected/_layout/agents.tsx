import { createFileRoute } from "@tanstack/react-router";
import { AgentPage } from "@/features/agents/AgentPage";

export const Route = createFileRoute("/_protected/_layout/agents")({
    component: AgentsRoute,
});

function AgentsRoute() {
    return <AgentPage />;
}
