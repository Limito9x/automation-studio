import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";
import type { UpsertProjectExecutorConfigDialogProps } from "./UpsertProjectExecutorConfigDialog";
import type { DeleteProjectExecutorConfigDialogProps } from "./DeleteProjectExecutorConfigDialog";

declare module "@/lib/dialog-registry" {
    interface GlobalDialogRegistry {
        "create-project": undefined;
        "update-project": { id: string };
        "delete-project": { id: string };
        "upsertProjectExecutorConfig": UpsertProjectExecutorConfigDialogProps;
        "deleteProjectExecutorConfig": DeleteProjectExecutorConfigDialogProps;
    }
}

const CreateProjectDialog = lazy(() =>
    import("./CreateProjectDialog").then((m) => ({
        default: m.CreateProjectDialog
    }))
);

const UpdateProjectDialog = lazy(() =>
    import("./UpdateProjectDialog").then((m) => ({
        default: m.UpdateProjectDialog
    }))
);

const DeleteProjectDialog = lazy(() =>
    import("./DeleteProjectDialog").then((m) => ({
        default: m.DeleteProjectDialog
    }))
);

const UpsertProjectExecutorConfigDialog = lazy(() =>
    import("./UpsertProjectExecutorConfigDialog").then((m) => ({
        default: m.UpsertProjectExecutorConfigDialog
    }))
);

const DeleteProjectExecutorConfigDialog = lazy(() =>
    import("./DeleteProjectExecutorConfigDialog").then((m) => ({
        default: m.DeleteProjectExecutorConfigDialog
    }))
);

registerDialog({
    id: "create-project",
    component: CreateProjectDialog
});

registerDialog({
    id: "update-project",
    component: UpdateProjectDialog
});

registerDialog({
    id: "delete-project",
    component: DeleteProjectDialog
});

registerDialog({
    id: "upsertProjectExecutorConfig",
    component: UpsertProjectExecutorConfigDialog
});

registerDialog({
    id: "deleteProjectExecutorConfig",
    component: DeleteProjectExecutorConfigDialog
});
