import { createFileRoute, redirect } from '@tanstack/react-router'

export const Route = createFileRoute('/_protected/_project/projects/$projectId/contents/')({
  beforeLoad: ({ params: { projectId } }) => {
    throw redirect({
      to: '/projects/$projectId/content-types',
      params: { projectId },
    })
  },
})
