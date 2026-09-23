import { createFileRoute, Outlet } from '@tanstack/react-router'

export const Route = createFileRoute('/_protected/_project/projects/$projectId/contents')({
  staticData: {
    breadcrumb: 'Contents',
  },
  component: () => <Outlet />,
})
