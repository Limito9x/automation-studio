using Automation.Workspace.Constants;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.CreateWorkspace;

public class CreateWorkspaceEndpoint(IMessageBus bus) : Endpoint<CreateWorkspaceCommand, WorkspaceDto>
{
    public override void Configure()
    {
        Post(WorkspaceRoutes.Workspaces);
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.Create);
    }

    public override async Task HandleAsync(CreateWorkspaceCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<WorkspaceDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
