namespace Automation.Pipeline.Features.Nodes.DeleteCustomNode;

public class DeleteCustomNodeEndpoint(IMessageBus bus)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("{id:guid}");
        Group<NodesGroup>();
        Description(x => x.WithName("DeleteCustomNode"));
        Permissions(P.Pipeline.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeleteCustomNodeCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
