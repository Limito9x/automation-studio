import { createFileRoute } from "@tanstack/react-router";
import { buildPagedSearchSchema } from "@/lib/schemas/pagedSearch.schema";
import { PROJECT_FILTERABLE_FIELDS } from "@/features/projects/schemas/projectFilterableFields";
import { ProjectPage } from "@/features/projects/ProjectPage";

export const projectsRouteSearch = buildPagedSearchSchema(PROJECT_FILTERABLE_FIELDS);

export const Route = createFileRoute("/_protected/_layout/projects/")({
    validateSearch: projectsRouteSearch,
    component: ProjectsRoute,
});

function ProjectsRoute() {
    return (
        <ProjectPage
            useSearch={Route.useSearch}
            useNavigate={Route.useNavigate}
        />
    );
}
