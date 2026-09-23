using Automation.Pipeline.Features.Nodes.CreateCustomNode;

namespace Automation.Pipeline.Features.Nodes.GetCustomNodeById;

public class GetCustomNodeByIdEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<CreateCustomNodeResponseDto>
{
    public override void Configure()
    {
        Get("custom/{Id:guid}");
        Group<NodesGroup>();
        Description(x => x.WithName("GetCustomNodeById"));
        Permissions(P.Pipeline.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var query = new GetCustomNodeByIdQuery(id);
        var result = await bus.InvokeAsync<Result<CreateCustomNodeResponseDto>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
