using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.GetAgents;

public class GetAgentsEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<AgentDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<AgentsGroup>();
        Permissions(P.Agent.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var isActive = Query<bool?>("isActive", isRequired: false);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<AgentDto>>>(new GetAgentsQuery(isActive), ct);
        await this.SendResultAsync(result, ct);
    }
}

