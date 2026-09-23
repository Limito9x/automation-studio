namespace Automation.Pipeline.Features.Pipelines.GetStructCatalogue;

public class GetStructCatalogueEndpoint(IMessageBus bus)
    : Endpoint<GetStructCatalogueQuery, IReadOnlyList<StructDefinitionDto>>
{
    public override void Configure()
    {
        Get("structs/catalogue");
        Group<PipelinesGroup>();
        Description(x => x.WithName("GetStructCatalogue"));
        Permissions(P.Pipeline.GetAll);
    }

    public override async Task HandleAsync(GetStructCatalogueQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<StructDefinitionDto>>>(
            req,
            ct
        );

        await this.SendResultAsync(result, ct);
    }
}
