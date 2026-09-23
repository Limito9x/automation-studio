namespace Automation.Agent.Features.Agents.RegisterAgentWithToken;

public record RegisterAgentWithTokenCommand(string SetupToken, string Name, string MachineKey);
