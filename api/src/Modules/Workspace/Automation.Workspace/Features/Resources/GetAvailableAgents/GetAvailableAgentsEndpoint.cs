namespace Automation.Workspace.Features.Resources.GetAvailableAgents;

public class GetAvailableAgentsEndpoint(IMessageBus bus)
    : Endpoint<GetAvailableAgentsQuery, List<AvailableAgentDto>>
{
    public override void Configure()
    {
        Post("/available-agents");
        Group<ResourcesGroup>();
        Permissions(P.Resource.GetAll);
    }

    public override async Task HandleAsync(GetAvailableAgentsQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<AvailableAgentDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
