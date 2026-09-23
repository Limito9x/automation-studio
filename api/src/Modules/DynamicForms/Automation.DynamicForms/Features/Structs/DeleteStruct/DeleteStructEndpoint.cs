namespace Automation.DynamicForms.Features.Structs.DeleteStruct;

public class DeleteStructEndpoint(IMessageBus bus)
    : Endpoint<DeleteStructCommand>
{
    public override void Configure()
    {
        Delete("{id:guid}");
        Group<StructsGroup>();
        Permissions(P.Structs.Delete);
        Description(x => x.WithName("DeleteStruct"));
    }

    public override async Task HandleAsync(DeleteStructCommand req, CancellationToken ct)
    {
        req = req with { Id = Route<Guid>("id"), ProjectId = Route<Guid>("projectId") };
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
