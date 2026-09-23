using Automation.Workspace.Features.Workspaces;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.WorkspaceAgents.CompareWorkspaceResources;

public class CompareWorkspaceResourceEndpoint(IMessageBus bus) : EndpointWithoutRequest<DiffResult>
{
    public override void Configure()
    {
        Post("/workspaces/{workspaceId:guid}/agents/{agentId:guid}/compare");
        Group<WorkspacesGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var workspaceId = Route<Guid>("workspaceId");
        var agentId = Route<Guid>("agentId");
        var cmd = new CompareWorkspaceResourcesCommand(workspaceId, agentId);
        var result = await bus.InvokeAsync<Result<DiffResult>>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
