import { createFileRoute } from "@tanstack/react-router";
import { ProjectExecutorSettingsPage } from "@/features/projects/ProjectExecutorSettingsPage";

export const Route = createFileRoute(
    "/_protected/_project/projects/$projectId/executor-settings"
)({
    staticData: {
        breadcrumb: "Executor Settings",
    },
    component: ExecutorSettingsRouteComponent,
});

function ExecutorSettingsRouteComponent() {
    const { projectId } = Route.useParams();
    return <ProjectExecutorSettingsPage projectId={projectId} />;
}
