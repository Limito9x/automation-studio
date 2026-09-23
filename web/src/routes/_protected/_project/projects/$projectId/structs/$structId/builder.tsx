import { createFileRoute } from '@tanstack/react-router'
import { StructSchemaBuilderPage } from '@/features/structs/StructSchemaBuilderPage'

export const Route = createFileRoute(
  '/_protected/_project/projects/$projectId/structs/$structId/builder',
)({
  component: RouteStructSchemaBuilderPage,
})

function RouteStructSchemaBuilderPage() {
  const { projectId, structId } = Route.useParams()
  return <StructSchemaBuilderPage projectId={projectId} structId={structId} />
}
