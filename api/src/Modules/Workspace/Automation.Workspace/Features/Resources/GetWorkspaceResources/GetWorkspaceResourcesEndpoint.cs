using Automation.SharedKernel.Abstractions.Querying;
using Automation.SharedKernel.Infrastructure.Querying;
using Automation.Workspace.Constants;
using Automation.Workspace.Features.Workspaces;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Resources.GetWorkspaceResources;

public class GetWorkspaceResourcesEndpoint(IMessageBus bus) : Endpoint<GetWorkspaceResourcesQuery, PagedResult<WorkspaceResourceDto>>
{
    public override void Configure()
    {
        Get(WorkspaceRoutes.WorkspaceResources);
        Group<WorkspacesGroup>();
        Permissions(P.Resource.GetAll);
    }

    public override async Task HandleAsync(GetWorkspaceResourcesQuery req, CancellationToken ct)
    {
        var workspaceId = Route<Guid>("workspaceId");
        var projectId = Query<Guid>("projectId");

        req.WorkspaceId = workspaceId;
        req.ProjectId = projectId;

        var result = await bus.InvokeAsync<Result<PagedResult<WorkspaceResourceDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
