using Automation.DynamicForms.Contracts;

namespace Automation.DynamicForms.Features.Structs.CreateStruct;

public class CreateStructEndpoint(IMessageBus bus)
    : Endpoint<CreateStructCommand, StructDetailDto>
{
    public override void Configure()
    {
        Post("");
        Group<StructsGroup>();
        Permissions(P.Structs.Create);
        Description(x => x.WithName("CreateStruct"));
    }

    public override async Task HandleAsync(CreateStructCommand req, CancellationToken ct)
    {
        req = req with { ProjectId = Route<Guid>("projectId") };
        var result = await bus.InvokeAsync<Result<StructDetailDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
