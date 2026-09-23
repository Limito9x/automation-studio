namespace Automation.Agent.Features.Agents.ScanExecutors;

public record ScanExecutorsCommand(
    Guid AgentId,
    string? ExecutorKey = null
);
