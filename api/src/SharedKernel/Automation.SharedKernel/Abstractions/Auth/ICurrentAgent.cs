namespace Automation.SharedKernel.Abstractions.Auth;

public interface ICurrentAgent
{
    Guid? AgentId { get; }
    bool IsAgentRequest { get; }
}

