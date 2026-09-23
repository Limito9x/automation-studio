using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Agent.Constants;

public class AgentPermissions
{
    public static AgentFeature Agent { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Agent", Agent.All }
    };

    public class AgentFeature() : BaseCrudPermission("agent") { }
}

