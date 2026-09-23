using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Workspace.Constants;

public class WorkspacePermissions
{
    public static WorkspaceFeature Workspace { get; } = new();
    public static WorkspaceAgentFeature WorkspaceAgent { get; } = new();
    public static ResourceFeature Resource { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Workspace", Workspace.All },
        { "WorkspaceAgent", WorkspaceAgent.All },
        { "Resource", Resource.All }
    };

    public class WorkspaceFeature() : BaseCrudPermission("workspace") { }
    public class WorkspaceAgentFeature() : BaseCrudPermission("workspace_agent") { }
    public class ResourceFeature() : BaseCrudPermission("resource") { }
}

