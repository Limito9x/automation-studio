namespace Automation.Agent.Features.Agents.DiscoverAgentFolder;

public class DiscoverAgentEndpoint(IMessageBus bus) : Endpoint<DiscoverAgentFolderQuery, DiscoverAgentFolderResult>
{
    public override void Configure()
    {
        Get("/{id:guid}/discover-folders");
        Group<AgentsGroup>();
        Permissions(P.Agent.GetAll);
        Description(x => x.WithName("DiscoverAgentFolders"));
    }

    public override async Task HandleAsync(DiscoverAgentFolderQuery query, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<DiscoverAgentFolderResult>>(
            query,
            ct
        );

        await this.SendResultAsync(result, ct);
    }
}
