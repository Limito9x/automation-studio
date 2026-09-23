import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";

declare module "@/lib/dialog-registry" {
    interface GlobalDialogRegistry {
        "delete-content-item": { id: string; typeKey: string; projectId: string };
        "link-content-resources": { contentId: string; contentName: string; projectId: string };
    }
}

const DeleteContentItemDialog = lazy(() =>
    import("./DeleteContentItemDialog").then((m) => ({
        default: m.DeleteContentItemDialog
    }))
);

const LinkContentResourcesDialog = lazy(() =>
    import("./LinkContentResourcesDialog").then((m) => ({
        default: m.LinkContentResourcesDialog
    }))
);

registerDialog({
    id: "delete-content-item",
    component: DeleteContentItemDialog
});

registerDialog({
    id: "link-content-resources",
    component: LinkContentResourcesDialog
});

