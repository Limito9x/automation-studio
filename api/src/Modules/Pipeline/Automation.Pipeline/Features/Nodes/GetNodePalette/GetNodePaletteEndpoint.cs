namespace Automation.Pipeline.Features.Nodes.GetNodePalette;

public class GetNodePaletteEndpoint(IMessageBus bus)
    : Endpoint<GetNodePaletteQuery, IReadOnlyList<NodePaletteItemDto>>
{
    public override void Configure()
    {
        Get("palette");
        Group<NodesGroup>();
        Description(x => x.WithName("GetNodePalette"));
        Permissions(P.Pipeline.GetAll);
    }

    public override async Task HandleAsync(GetNodePaletteQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<NodePaletteItemDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
