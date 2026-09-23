import { createFileRoute } from "@tanstack/react-router";
import { PipelineEditorPage } from "@/features/pipelines/pages/PipelineEditorPage";

export const Route = createFileRoute(
  "/_protected/_project/projects/$projectId/pipeline/$pipelineId"
)({
  staticData: {
    breadcrumb: "Pipeline Editor",
  },
  component: PipelineEditorRoute,
});

function PipelineEditorRoute() {
  const { projectId, pipelineId } = Route.useParams();
  return <PipelineEditorPage projectId={projectId} pipelineId={pipelineId} />;
}
