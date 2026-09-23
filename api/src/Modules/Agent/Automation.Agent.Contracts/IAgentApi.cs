using FluentResults;

namespace Automation.Agent.Contracts;

public interface IAgentApi
{
    Task<Result<AgentDto>> GetAgentByIdAsync(Guid agentId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<AgentDto>>> GetAgentsByIdsAsync(
        IEnumerable<Guid> agentIds,
        CancellationToken ct = default
    );
    Task<Result<IReadOnlyDictionary<Guid, AgentDto>>> GetAgentsMapByIdsAsync(
        IEnumerable<Guid> agentIds,
        CancellationToken ct = default
    );
    Task<Result<AgentScanResultDto>> SendScanCommandAsync(
        Guid agentId,
        string directoryPath,
        IEnumerable<string>? extensions = null,
        CancellationToken ct = default
    );
    Task<Result<AgentBrowseResultDto>> SendBrowseCommandAsync(
        Guid agentId,
        string directoryPath,
        CancellationToken ct = default
    );
    Task<Result<IReadOnlyList<ExecutorCandidateDto>>> SendScanExecutorsCommandAsync(
        Guid agentId,
        string? executorKey = null,
        CancellationToken ct = default
    );
    Task<Result<List<AgentInfo>>> GetAgentInfoByIds(
        IReadOnlyList<Guid> agentIds,
        CancellationToken ct = default
    );
    Task<Result<List<AgentDto>>> GetAvailableAgentsByUserId(
        Guid userId,
        CancellationToken ct = default
    );
}
