using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.ReportExecutorConfigs;

public class ReportExecutorConfigsEndpoint(IMessageBus bus) : Endpoint<ReportExecutorConfigsCommand, IReadOnlyList<AgentExecutorConfigDto>>
{
    public override void Configure()
    {
        Post("/{agentId:guid}/executor-configs");
        Group<AgentsGroup>();
        Permissions(P.Agent.Update);
    }

    public override async Task HandleAsync(ReportExecutorConfigsCommand req, CancellationToken ct)
    {
        var agentId = Route<Guid>("agentId");
        var command = req with { AgentId = agentId };
        var result = await bus.InvokeAsync<Result<IReadOnlyList<AgentExecutorConfigDto>>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
