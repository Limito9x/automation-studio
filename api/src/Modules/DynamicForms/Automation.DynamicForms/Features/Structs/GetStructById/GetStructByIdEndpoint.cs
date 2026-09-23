using Automation.DynamicForms.Contracts;

namespace Automation.DynamicForms.Features.Structs.GetStructById;

public class GetStructByIdEndpoint(IMessageBus bus)
    : Endpoint<GetStructByIdQuery, StructDetailDto>
{
    public override void Configure()
    {
        Get("{id:guid}");
        Group<StructsGroup>();
        Permissions(P.Structs.GetById);
        Description(x => x.WithName("GetStructById"));
    }

    public override async Task HandleAsync(GetStructByIdQuery req, CancellationToken ct)
    {
        req = req with { Id = Route<Guid>("id"), ProjectId = Route<Guid>("projectId") };
        var result = await bus.InvokeAsync<Result<StructDetailDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
