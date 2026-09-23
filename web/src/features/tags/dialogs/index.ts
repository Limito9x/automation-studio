import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";
import type { CreateTagData } from "./CreateTagDialog";

declare module "@/lib/dialog-registry" {
    interface GlobalDialogRegistry {
        "create-tag": CreateTagData;
    }
}

const CreateTagDialog = lazy(() =>
    import("./CreateTagDialog").then((m) => ({
        default: m.CreateTagDialog,
    }))
);

registerDialog({
    id: "create-tag",
    component: CreateTagDialog,
});
