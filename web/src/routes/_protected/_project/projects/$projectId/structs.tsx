import { createFileRoute, Outlet } from '@tanstack/react-router'

export const Route = createFileRoute('/_protected/_project/projects/$projectId/structs')({
  staticData: {
    breadcrumb: 'Structs',
  },
  component: () => <Outlet />,
})
