using Automation.Workspace.Constants;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaceById;

public class GetWorkspaceByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<WorkspaceDetailDto>
{
    public override void Configure()
    {
        Get(WorkspaceRoutes.Workspace);
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<WorkspaceDetailDto>>(new GetWorkspaceByIdQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
