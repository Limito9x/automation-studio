import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";

declare module "@/lib/dialog-registry" {
    interface GlobalDialogRegistry {
        "delete-user": { id: string };
        "assign-user-roles": { id: string };
    }
}

const DeleteUserDialog = lazy(() =>
    import("./DeleteUserDialog").then((m) => ({
        default: m.DeleteUserDialog
    }))
);

const AssignUserRolesDialog = lazy(() =>
    import("./AssignUserRolesDialog").then((m) => ({
        default: m.AssignUserRolesDialog
    }))
);

registerDialog({
    id: "delete-user",
    component: DeleteUserDialog
});

registerDialog({
    id: "assign-user-roles",
    component: AssignUserRolesDialog
});
