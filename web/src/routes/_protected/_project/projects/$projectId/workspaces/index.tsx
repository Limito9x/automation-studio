import { createFileRoute } from '@tanstack/react-router'
import { WorkspaceListPage } from '@/features/workspaces/pages/WorkspaceListPage'

export const Route = createFileRoute('/_protected/_project/projects/$projectId/workspaces/')({
  component: WorkspacesRouteComponent,
})

function WorkspacesRouteComponent() {
  const { projectId } = Route.useParams()
  return <WorkspaceListPage projectId={projectId} />
}
