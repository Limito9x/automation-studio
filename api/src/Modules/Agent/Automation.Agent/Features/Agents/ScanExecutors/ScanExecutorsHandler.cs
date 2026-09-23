using Automation.Agent.Contracts;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.ScanExecutors;

[NonTransactional]
public class ScanExecutorsHandler(IAgentApi agentApi)
{
    public async Task<Result<IReadOnlyList<ExecutorCandidateDto>>> HandleAsync(ScanExecutorsCommand command, CancellationToken ct)
    {
        return await agentApi.SendScanExecutorsCommandAsync(command.AgentId, command.ExecutorKey, ct);
    }
}
