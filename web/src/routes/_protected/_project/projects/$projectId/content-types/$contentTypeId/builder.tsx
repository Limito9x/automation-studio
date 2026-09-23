import { createFileRoute } from '@tanstack/react-router'
import { ContentTypeSchemaBuilderPage } from '@/features/contentTypes/ContentTypeSchemaBuilderPage'

export const Route = createFileRoute('/_protected/_project/projects/$projectId/content-types/$contentTypeId/builder')({
    component: RouteContentTypeSchemaBuilderPage,
})

function RouteContentTypeSchemaBuilderPage() {
    const { projectId, contentTypeId } = Route.useParams();
    return <ContentTypeSchemaBuilderPage projectId={projectId} contentTypeId={contentTypeId} />
}