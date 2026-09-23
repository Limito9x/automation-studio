import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";
import type { ContentTypeDto } from "@/gen/model";

declare module "@/lib/dialog-registry" {
    interface GlobalDialogRegistry {
        "delete-content-type": { id: string; projectId?: string };
        "create-content-type": { projectId: string };
        "update-content-type": { item: ContentTypeDto };
    }
}

const DeleteContentTypeDialog = lazy(() =>
    import("./DeleteContentTypeDialog").then((m) => ({
        default: m.DeleteContentTypeDialog
    }))
);

const CreateContentTypeDialog = lazy(() =>
    import("./CreateContentTypeDialog").then((m) => ({
        default: m.CreateContentTypeDialog
    }))
);

const UpdateContentTypeDialog = lazy(() =>
    import("./UpdateContentTypeDialog").then((m) => ({
        default: m.UpdateContentTypeDialog
    }))
);

registerDialog({
    id: "delete-content-type",
    component: DeleteContentTypeDialog
});

registerDialog({
    id: "create-content-type",
    component: CreateContentTypeDialog
});

registerDialog({
    id: "update-content-type",
    component: UpdateContentTypeDialog
});
