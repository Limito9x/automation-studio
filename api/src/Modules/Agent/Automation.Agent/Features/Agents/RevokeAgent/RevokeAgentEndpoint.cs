namespace Automation.Agent.Features.Agents.RevokeAgent;

public class RevokeAgentEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/{id:guid}/revoke");
        Group<AgentsGroup>();
        Permissions(P.Agent.Update);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new RevokeAgentCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

