using Automation.DynamicForms.Contracts;

namespace Automation.DynamicForms.Features.Structs.GetProjectStructs;

public class GetProjectStructsEndpoint(IMessageBus bus)
    : Endpoint<GetProjectStructsQuery, IReadOnlyList<StructSummaryDto>>
{
    public override void Configure()
    {
        Get("");
        Group<StructsGroup>();
        Permissions(P.Structs.GetAll);
        Description(x => x.WithName("GetProjectStructs"));
    }

    public override async Task HandleAsync(GetProjectStructsQuery req, CancellationToken ct)
    {
        req = req with { ProjectId = Route<Guid>("projectId") };
        var result = await bus.InvokeAsync<Result<IReadOnlyList<StructSummaryDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
