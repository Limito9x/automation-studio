namespace Automation.Agent.Contracts;

public record AgentExecutorConfigDto(
    Guid Id,
    Guid AgentId,
    string ExecutorKey,
    string ExecutablePath,
    string? Version,
    DateTimeOffset CreatedAt
);

public record AgentInfo(
    Guid Id,
    string Name,
    bool IsAvailable,
    List<AgentExecutorConfigInfo> ExecutorConfigs
);

public record AgentExecutorConfigInfo(string Key, string ExecutablePath, string? Version);
