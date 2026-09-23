import { createFileRoute } from '@tanstack/react-router'
import { StructsPage } from '@/features/structs/StructsPage'

export const Route = createFileRoute('/_protected/_project/projects/$projectId/structs/')({
  component: StructsRoute,
})

function StructsRoute() {
  const { projectId } = Route.useParams()
  return <StructsPage projectId={projectId} />
}
