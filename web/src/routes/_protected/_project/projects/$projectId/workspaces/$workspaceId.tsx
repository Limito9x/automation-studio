import { createFileRoute } from '@tanstack/react-router'
import { WorkspaceDetailPage } from '@/features/workspaces/pages/WorkspaceDetailPage'
import { buildPagedSearchSchema } from '@/lib/schemas/pagedSearch.schema'
import { getWorkspaceById } from '@/gen/endpoints/workspaces/workspaces'
import { z } from 'zod'

const baseSchema = buildPagedSearchSchema({
  name: ['Equal', 'Contains'],
  filePath: ['Equal', 'Contains'],
  createdAt: ['GreaterThan', 'GreaterThanOrEqual', 'LessThan', 'LessThanOrEqual'],
  resourceName: ['Equal', 'Contains'],
  relativePath: ['Equal', 'Contains'],
})

export const workspaceDetailSearchSchema = baseSchema.extend({
  tab: z.enum(['resources', 'changes']).catch('resources').default('resources'),
  agentId: z.string().optional(),
})

export const Route = createFileRoute('/_protected/_project/projects/$projectId/workspaces/$workspaceId')({
  validateSearch: workspaceDetailSearchSchema,
  loader: async ({ params: { workspaceId } }) => {
    try {
      const workspace = await getWorkspaceById(workspaceId);
      return { workspace };
    } catch {
      return { workspace: null };
    }
  },
  component: WorkspaceDetailRouteComponent,
})

function WorkspaceDetailRouteComponent() {
  const { projectId, workspaceId } = Route.useParams()
  return (
    <WorkspaceDetailPage 
      projectId={projectId} 
      workspaceId={workspaceId} 
      useSearch={Route.useSearch} 
      useNavigate={Route.useNavigate} 
    />
  )
}
