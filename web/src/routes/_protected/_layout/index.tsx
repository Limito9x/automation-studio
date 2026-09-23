import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_protected/_layout/')({
  staticData: {
    breadcrumb: 'Dashboard',
  },
  component: DashboardPage,
})

function DashboardPage() {
  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
      <p className="text-muted-foreground mt-2">Welcome to your new workspace.</p>
    </div>
  )
}
