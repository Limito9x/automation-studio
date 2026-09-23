using Automation.Agent.Contracts;

namespace Automation.Agent.Features.Agents.ScanExecutors;

public class ScanExecutorsEndpoint(IMessageBus bus) : Endpoint<ScanExecutorsCommand, IReadOnlyList<ExecutorCandidateDto>>
{
    public override void Configure()
    {
        Post("/{agentId:guid}/executors/scan");
        Group<AgentsGroup>();
        Permissions(P.Agent.Update);
    }

    public override async Task HandleAsync(ScanExecutorsCommand req, CancellationToken ct)
    {
        var agentId = Route<Guid>("agentId");
        var command = req with { AgentId = agentId };
        var result = await bus.InvokeAsync<Result<IReadOnlyList<ExecutorCandidateDto>>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
