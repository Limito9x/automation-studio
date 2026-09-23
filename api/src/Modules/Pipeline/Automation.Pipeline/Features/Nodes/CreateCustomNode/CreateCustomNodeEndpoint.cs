namespace Automation.Pipeline.Features.Nodes.CreateCustomNode;

public class CreateCustomNodeEndpoint(IMessageBus bus)
    : Endpoint<CreateCustomNodeCommand, CreateCustomNodeResponseDto>
{
    public override void Configure()
    {
        Post("custom");
        Group<NodesGroup>();
        Description(x => x.WithName("CreateCustomNode"));
        Permissions(P.Pipeline.Create);
    }

    public override async Task HandleAsync(CreateCustomNodeCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<CreateCustomNodeResponseDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
