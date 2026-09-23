namespace Automation.Agent.Domain.Entities;

public class AgentExecutorConfig : AuditableEntity
{
    public Guid AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public string ExecutorKey { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;

    public AgentExecutorConfig() { }

    public AgentExecutorConfig(Guid agentId, string executorKey, string executablePath, string? version = null)
    {
        AgentId = agentId;
        ExecutorKey = executorKey;
        ExecutablePath = executablePath;
        Version = version;
    }

    public void Update(string executablePath, string? version)
    {
        ExecutablePath = executablePath;
        Version = version;
    }
}
