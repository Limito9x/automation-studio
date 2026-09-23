import { createFileRoute, Outlet } from '@tanstack/react-router'
import { ProjectShell } from '@/components/layout/app/ProjectShell'

export const Route = createFileRoute('/_protected/_project')({
    component: RouteComponent,
})

function RouteComponent() {
    return (
        <ProjectShell>
            <Outlet />
        </ProjectShell>
    )
}
