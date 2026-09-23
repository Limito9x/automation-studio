import { createFileRoute } from "@tanstack/react-router";
import { PipelineListPage } from "@/features/pipelines/pages/PipelineListPage";

export const Route = createFileRoute(
  "/_protected/_project/projects/$projectId/pipeline/"
)({
  staticData: {
    breadcrumb: "Pipelines",
  },
  component: PipelineListRoute,
});

function PipelineListRoute() {
  const { projectId } = Route.useParams();
  return <PipelineListPage projectId={projectId} />;
}
