using System.Text.Json;

namespace Automation.Projects.Features.ProjectExecutorConfigs.UpsertProjectExecutorConfig;

public record UpsertProjectExecutorConfigCommand(
    Guid ProjectId,
    Guid AgentId,
    string ExecutorKey,
    JsonDocument? Settings
);
