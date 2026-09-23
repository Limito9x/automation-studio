namespace Automation.Agent.Features.Connections;

public interface IAgentConnectionRegistry
{
    void Add(AgentConnection connection);

    bool TryGet(Guid agentId, out AgentConnection? connection);

    bool Remove(Guid agentId, Guid connectionId);

    bool Contain(Guid agentId);
}
