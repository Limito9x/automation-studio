using Automation.Pipeline.Features.Nodes.CreateCustomNode;

namespace Automation.Pipeline.Features.Nodes.UpdateCustomNode;

public class UpdateCustomNodeEndpoint(IMessageBus bus)
    : Endpoint<UpdateCustomNodeRequest, CreateCustomNodeResponseDto>
{
    public override void Configure()
    {
        Put("custom/{Id:guid}");
        Group<NodesGroup>();
        Description(x => x.WithName("UpdateCustomNode"));
        Permissions(P.Pipeline.Update);
    }

    public override async Task HandleAsync(UpdateCustomNodeRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var cmd = new UpdateCustomNodeCommand(
            id,
            req.Name,
            req.Label,
            req.Executor,
            req.AssetId,
            req.OriginalFileName,
            req.Inputs,
            req.Outputs
        );

        var result = await bus.InvokeAsync<Result<CreateCustomNodeResponseDto>>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
