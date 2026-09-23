namespace Automation.Agent.Features.Connections;

using System.Collections.Concurrent;

public sealed class AgentConnectionRegistry : IAgentConnectionRegistry
{
    private readonly ConcurrentDictionary<Guid, AgentConnection> _connections = new();

    public void Add(AgentConnection connection)
    {
        _connections[connection.AgentId] = connection;
    }

    public bool TryGet(Guid agentId, out AgentConnection? connection)
    {
        return _connections.TryGetValue(agentId, out connection);
    }

    public bool Remove(Guid agentId, Guid connectionId)
    {
        if (!_connections.TryGetValue(agentId, out var connection))
            return false;

        if (connection.ConnectionId != connectionId)
            return false;

        return _connections.TryRemove(agentId, out _);
    }

    public bool Contain(Guid agentId)
    {
        return _connections.ContainsKey(agentId);
    }
}
