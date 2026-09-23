import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";

declare module "@/lib/dialog-registry" {
    interface GlobalDialogRegistry {
        "create-platform": undefined;
        "update-platform": { id: string };
        "delete-platform": { id: string };
        "create-extension": undefined;
        "delete-extension": { id: string };
    }
}

const CreatePlatformDialog = lazy(() =>
    import("./CreatePlatformDialog").then((m) => ({
        default: m.CreatePlatformDialog
    }))
);

const UpdatePlatformDialog = lazy(() =>
    import("./UpdatePlatformDialog").then((m) => ({
        default: m.UpdatePlatformDialog
    }))
);

const DeletePlatformDialog = lazy(() =>
    import("./DeletePlatformDialog").then((m) => ({
        default: m.DeletePlatformDialog
    }))
);

const CreateExtensionDialog = lazy(() =>
    import("./CreateExtensionDialog").then((m) => ({
        default: m.CreateExtensionDialog
    }))
);

const DeleteExtensionDialog = lazy(() =>
    import("./DeleteExtensionDialog").then((m) => ({
        default: m.DeleteExtensionDialog
    }))
);

registerDialog({
    id: "create-platform",
    component: CreatePlatformDialog
});

registerDialog({
    id: "update-platform",
    component: UpdatePlatformDialog
});

registerDialog({
    id: "delete-platform",
    component: DeletePlatformDialog
});

registerDialog({
    id: "create-extension",
    component: CreateExtensionDialog
});

registerDialog({
    id: "delete-extension",
    component: DeleteExtensionDialog
});
