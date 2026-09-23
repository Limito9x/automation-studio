using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.ReportExecutorConfigs;

public record ExecutorConfigItemInput(
    string ExecutorKey,
    string ExecutablePath,
    string? Version
);

public record ReportExecutorConfigsCommand(
    Guid AgentId,
    IReadOnlyList<ExecutorConfigItemInput> Configs
);
