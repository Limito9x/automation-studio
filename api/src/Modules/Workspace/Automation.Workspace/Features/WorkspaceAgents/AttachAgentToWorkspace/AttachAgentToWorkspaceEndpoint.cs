using Automation.Workspace.Constants;
using Automation.Workspace.Features.Workspaces;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.WorkspaceAgents.AttachAgentToWorkspace;

public class AttachAgentToWorkspaceEndpoint(IMessageBus bus) : Endpoint<AttachAgentToWorkspaceCommand, WorkspaceAgentDto>
{
    public override void Configure()
    {
        Post(WorkspaceRoutes.AttachAgent);
        Group<WorkspacesGroup>();
        Permissions(P.WorkspaceAgent.Create);
    }

    public override async Task HandleAsync(AttachAgentToWorkspaceCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<WorkspaceAgentDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
