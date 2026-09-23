namespace Automation.Workspace.Constants;

public static class WorkspaceRoutes
{
    public const string NestedWorkspaces = "/projects/{projectId:guid}/workspaces";
    public const string Workspaces = "/workspaces";
    public const string Workspace = "/workspaces/{id:guid}";
    public const string AttachAgent = "/workspaces/{workspaceId:guid}/agents";
    public const string ScanFiles = "/workspaces/{workspaceId:guid}/agents/{agentId:guid}/scan-files";
    public const string WorkspaceResources = "/workspaces/{workspaceId:guid}/resources";
    public const string WorkspaceAgentResources = "/workspaces/{workspaceId:guid}/agents/{agentId:guid}/resources";
    public const string AssignResourcesContent = "/workspaces/resources/assign-content";
    public const string ContentResources = "/workspaces/resources/by-content/{contentId:guid}";
}
