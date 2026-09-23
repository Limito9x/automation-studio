using Automation.Agent.Shared.Dtos;
using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Agent.Features.Agents.GetMyAgent;

public class GetMyAgentEndpoint(IMessageBus bus, ICurrentAgent currentAgent) : EndpointWithoutRequest<AgentDto>
{
    public override void Configure()
    {
        Get("/me");
        Group<AgentsGroup>();
        AllowAnonymous(); // Auth via X-Agent-Key header
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!currentAgent.IsAgentRequest || !currentAgent.AgentId.HasValue)
        {
            await this.SendResultAsync(Result.Fail("Unauthorized"), ct);
            return;
        }

        var result = await bus.InvokeAsync<Result<AgentDto>>(new GetMyAgentQuery(currentAgent.AgentId.Value), ct);
        await this.SendResultAsync(result, ct);
    }
}

