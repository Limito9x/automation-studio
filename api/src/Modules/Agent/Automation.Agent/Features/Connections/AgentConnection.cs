using Automation.Agent.Grpc;
using Grpc.Core;

namespace Automation.Agent.Features.Connections;

public class AgentConnection{
    public Guid AgentId { get;}
    public Guid ConnectionId {get;}
    public IServerStreamWriter<ServerMessage> ResponseStream { get; set; } = null!;
    public IAsyncStreamReader<AgentMessage> RequestStream { get; set; } = null!;

    public AgentConnection(Guid agentId, Guid connectionId, IServerStreamWriter<ServerMessage> responseStream, IAsyncStreamReader<AgentMessage> requestStream){
        AgentId = agentId;
        ConnectionId = connectionId;
        ResponseStream = responseStream;
        RequestStream = requestStream;
    }
}