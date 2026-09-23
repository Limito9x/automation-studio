namespace Automation.Agent.Shared.Dtos;

public record AgentDto(
    Guid Id,
    string Name,
    string MachineKey,
    bool IsActive,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<AgentExecutorConfigDto>? ExecutorConfigs = null
);

public record RegisterAgentResultDto(
    Guid Id,
    string Name,
    string MachineKey,
    string RegistrationToken
);

public record AgentExecutorConfigDto(
    Guid Id,
    Guid AgentId,
    string ExecutorKey,
    string ExecutablePath,
    string? Version,
    DateTimeOffset CreatedAt
);
