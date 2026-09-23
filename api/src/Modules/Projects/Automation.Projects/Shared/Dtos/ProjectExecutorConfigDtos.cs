using System.Text.Json;

namespace Automation.Projects.Shared.Dtos;

public record ProjectExecutorConfigDto(
    Guid Id,
    Guid ProjectId,
    Guid AgentId,
    string ExecutorKey,
    JsonDocument? Settings,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record UpsertProjectExecutorConfigDto(
    Guid ProjectId,
    Guid AgentId,
    string ExecutorKey,
    JsonDocument? Settings
);
