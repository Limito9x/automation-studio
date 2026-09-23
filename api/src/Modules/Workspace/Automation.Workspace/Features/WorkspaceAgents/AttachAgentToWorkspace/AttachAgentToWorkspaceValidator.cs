namespace Automation.Workspace.Features.WorkspaceAgents.AttachAgentToWorkspace;

public class AttachAgentToWorkspaceValidator : Validator<AttachAgentToWorkspaceCommand>
{
    public AttachAgentToWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.RootPath).NotEmpty().MaximumLength(500);
    }
}
