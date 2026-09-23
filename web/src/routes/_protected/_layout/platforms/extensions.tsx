import { createFileRoute } from "@tanstack/react-router";
import { buildPagedSearchSchema } from "@/lib/schemas/pagedSearch.schema";
import { ExtensionPage } from "@/features/platforms/ExtensionPage";

export const extensionsRouteSearch = buildPagedSearchSchema({});

export const Route = createFileRoute("/_protected/_layout/platforms/extensions")({
    validateSearch: extensionsRouteSearch,
    component: ExtensionsRoute,
});

function ExtensionsRoute() {
    return (
        <ExtensionPage
            useSearch={Route.useSearch}
            useNavigate={Route.useNavigate}
        />
    );
}
