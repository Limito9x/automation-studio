namespace Automation.Workspace.Features.WorkspaceAgents.AttachAgentToWorkspace;

public record AttachAgentToWorkspaceCommand(
    Guid WorkspaceId,
    Guid AgentId,
    string RootPath
);
