using Automation.DynamicForms.Contracts;

namespace Automation.DynamicForms.Features.Structs.UpdateStruct;

public class UpdateStructEndpoint(IMessageBus bus)
    : Endpoint<UpdateStructCommand, StructDetailDto>
{
    public override void Configure()
    {
        Put("{id:guid}");
        Group<StructsGroup>();
        Permissions(P.Structs.Update);
        Description(x => x.WithName("UpdateStruct"));
    }

    public override async Task HandleAsync(UpdateStructCommand req, CancellationToken ct)
    {
        req = req with { Id = Route<Guid>("id"), ProjectId = Route<Guid>("projectId") };
        var result = await bus.InvokeAsync<Result<StructDetailDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
