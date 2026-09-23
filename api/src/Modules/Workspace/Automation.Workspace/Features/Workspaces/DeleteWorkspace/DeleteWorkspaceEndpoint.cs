using Automation.Workspace.Constants;

namespace Automation.Workspace.Features.Workspaces.DeleteWorkspace;

public class DeleteWorkspaceEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete(WorkspaceRoutes.Workspace);
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeleteWorkspaceCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
