using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.ConfigureAgentExecutor;

public class ConfigureAgentExecutorEndpoint(IMessageBus bus) : Endpoint<ConfigureAgentExecutorCommand, AgentExecutorConfigDto>
{
    public override void Configure()
    {
        Post("/{agentId:guid}/executors");
        Group<AgentsGroup>();
        Permissions(P.Agent.Update);
    }

    public override async Task HandleAsync(ConfigureAgentExecutorCommand req, CancellationToken ct)
    {
        var agentId = Route<Guid>("agentId");
        var command = req with { AgentId = agentId };
        var result = await bus.InvokeAsync<Result<AgentExecutorConfigDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
