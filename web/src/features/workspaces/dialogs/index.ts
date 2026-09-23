import { lazy } from "react";
import { registerDialog } from "@/lib/dialog-registry";

declare module "@/lib/dialog-registry" {
  interface GlobalDialogRegistry {
    "create-workspace": { projectId: string };
    "update-workspace": { id: string; name: string; platformIds?: string[] };
    "delete-workspace": { id: string; name: string };
    "attach-agent-workspace": { workspaceId: string };
  }
}

const CreateWorkspaceDialog = lazy(() =>
  import("./CreateWorkspaceDialog").then((m) => ({ default: m.CreateWorkspaceDialog }))
);

const UpdateWorkspaceDialog = lazy(() =>
  import("./UpdateWorkspaceDialog").then((m) => ({ default: m.UpdateWorkspaceDialog }))
);

const DeleteWorkspaceDialog = lazy(() =>
  import("./DeleteWorkspaceDialog").then((m) => ({ default: m.DeleteWorkspaceDialog }))
);

const AttachAgentDialog = lazy(() =>
  import("./AttachAgentDialog").then((m) => ({ default: m.AttachAgentDialog }))
);

registerDialog({
  id: "create-workspace",
  component: CreateWorkspaceDialog,
});

registerDialog({
  id: "update-workspace",
  component: UpdateWorkspaceDialog,
});

registerDialog({
  id: "delete-workspace",
  component: DeleteWorkspaceDialog,
});

registerDialog({
  id: "attach-agent-workspace",
  component: AttachAgentDialog,
});
