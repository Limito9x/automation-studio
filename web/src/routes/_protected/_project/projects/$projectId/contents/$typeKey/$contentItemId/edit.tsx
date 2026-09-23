import { createFileRoute } from '@tanstack/react-router';
import { UpdateContentItemPage } from '@/features/contentItems/UpdateContentItemPage';

export const Route = createFileRoute('/_protected/_project/projects/$projectId/contents/$typeKey/$contentItemId/edit')({
    component: ContentItemEditRoute,
});

function ContentItemEditRoute() {
    return <UpdateContentItemPage />;
}
