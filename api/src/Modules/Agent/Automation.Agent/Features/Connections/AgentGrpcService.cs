using Automation.Agent.Grpc;
using Grpc.Core;

namespace Automation.Agent.Features.Connections;

public class AgentGrpcService(
    IAgentConnectionRegistry registry,
    ICommandTracker commandTracker) : AgentService.AgentServiceBase
{
    private readonly IAgentConnectionRegistry _registry = registry;
    private readonly ICommandTracker _commandTracker = commandTracker;

    public override async Task Connect(
        IAsyncStreamReader<AgentMessage> requestStream,
        IServerStreamWriter<ServerMessage> responseStream,
        ServerCallContext context)
    {
        try
        {
            if (!await requestStream.MoveNext(context.CancellationToken))
                return;

            var agentId = Guid.Parse(requestStream.Current.AgentId);

            var connectionId = Guid.NewGuid();
            var connection = new AgentConnection(
                agentId,
                connectionId,
                responseStream,
                requestStream
            );

            _registry.Add(connection);

            try
            {
                while (!context.CancellationToken.IsCancellationRequested &&
                       await requestStream.MoveNext(context.CancellationToken))
                {
                    var message = requestStream.Current;
                    if (message.PayloadCase == AgentMessage.PayloadOneofCase.CommandResponse)
                    {
                        _commandTracker.CompleteCommand(message.CommandResponse);
                    }
                }
            }
            finally
            {
                _registry.Remove(agentId, connectionId);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal graceful shutdown or client disconnect
        }
        catch (IOException)
        {
            // Connection closed by host shutdown
        }
    }
}