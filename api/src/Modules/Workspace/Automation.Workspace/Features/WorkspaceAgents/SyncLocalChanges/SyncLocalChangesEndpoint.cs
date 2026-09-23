using Automation.Workspace.Features.Workspaces;

namespace Automation.Workspace.Features.WorkspaceAgents.SyncLocalChanges;

public class SyncLocalChangesEndpoint(IMessageBus bus)
    : Endpoint<SyncLocalChangesCommand, SyncLocalChangesResult>
{
    public override void Configure()
    {
        Post("/workspaces/{workspaceId:guid}/agents/{agentId:guid}/sync-local-changes");
        Group<WorkspacesGroup>();
    }

    public override async Task HandleAsync(SyncLocalChangesCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<SyncLocalChangesResult>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
