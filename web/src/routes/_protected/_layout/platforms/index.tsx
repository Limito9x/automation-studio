import { createFileRoute } from "@tanstack/react-router";
import { buildPagedSearchSchema } from "@/lib/schemas/pagedSearch.schema";
import { PlatformPage } from "@/features/platforms/PlatformPage";

export const platformsRouteSearch = buildPagedSearchSchema({});

export const Route = createFileRoute("/_protected/_layout/platforms/")({
    validateSearch: platformsRouteSearch,
    component: PlatformsRoute,
});

function PlatformsRoute() {
    return (
        <PlatformPage
            useSearch={Route.useSearch}
            useNavigate={Route.useNavigate}
        />
    );
}
