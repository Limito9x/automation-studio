import { ProjectExecutorConfigTable } from "./components/ProjectExecutorConfigTable";

interface ProjectExecutorSettingsPageProps {
    projectId: string;
}

export function ProjectExecutorSettingsPage({ projectId }: ProjectExecutorSettingsPageProps) {
    return (
        <div className="flex-1 p-6 space-y-6 max-w-5xl mx-auto w-full">
            <ProjectExecutorConfigTable projectId={projectId} />
        </div>
    );
}
