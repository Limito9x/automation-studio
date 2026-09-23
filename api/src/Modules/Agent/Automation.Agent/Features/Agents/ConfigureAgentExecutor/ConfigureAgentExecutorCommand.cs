namespace Automation.Agent.Features.Agents.ConfigureAgentExecutor;

public record ConfigureAgentExecutorCommand(
    Guid AgentId,
    string ExecutorKey,
    string ExecutablePath,
    string? Version
);
