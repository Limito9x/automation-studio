namespace Automation.Agent.Domain.Entities;

public class Agent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string MachineKey { get; set; } = string.Empty;
    public string RegistrationToken { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastSeenAt { get; set; }

    public ICollection<AgentExecutorConfig> ExecutorConfigs { get; set; } = new List<AgentExecutorConfig>();

    public Agent() { }

    public Agent(string name, string machineKey, string registrationToken)
    {
        Name = name;
        MachineKey = machineKey;
        RegistrationToken = registrationToken;
        IsActive = true;
    }

    public void UpdateLastSeen()
    {
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
}

