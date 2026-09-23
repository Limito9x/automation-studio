import { createFileRoute } from "@tanstack/react-router";
import { buildPagedSearchSchema } from "@/lib/schemas/pagedSearch.schema";
import { CONTENT_TYPE_FILTERABLE_FIELDS } from "@/features/contentTypes/schemas/contentTypeFilterableFields";
import { ContentTypePage } from "@/features/contentTypes/ContentTypePage";

export const contentTypesRouteSearch = buildPagedSearchSchema(CONTENT_TYPE_FILTERABLE_FIELDS);

export const Route = createFileRoute("/_protected/_project/projects/$projectId/content-types/")({
    validateSearch: contentTypesRouteSearch,
    component: ContentTypesRoute,
});

function ContentTypesRoute() {
    const params = Route.useParams();

    return (
        <ContentTypePage
            projectId={params.projectId}
            useSearch={Route.useSearch}
            useNavigate={Route.useNavigate}
        />
    );
}
