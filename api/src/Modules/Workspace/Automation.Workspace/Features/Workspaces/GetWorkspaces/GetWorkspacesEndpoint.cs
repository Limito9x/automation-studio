using Automation.Workspace.Constants;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaces;

public class GetWorkspacesEndpoint(IMessageBus bus) : Endpoint<GetWorkspacesQuery, IReadOnlyList<WorkspaceDto>>
{
    public override void Configure()
    {
        Get(WorkspaceRoutes.NestedWorkspaces);
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.GetAll);
        Description(x => x.WithName("GetWorkspaces"));
    }

    public override async Task HandleAsync(GetWorkspacesQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<WorkspaceDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
