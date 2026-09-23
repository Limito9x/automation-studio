namespace Automation.Pipeline.Features.Pipelines.GetPinCatalogue;

public class GetPinCatalogueEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<IReadOnlyList<PinTypeMetadataDto>>
{
    public override void Configure()
    {
        Get("pins/catalogue");
        Group<PipelinesGroup>();
        Description(x => x.WithName("GetPinCatalogue"));
        Permissions(P.Pipeline.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<PinTypeMetadataDto>>>(
            new GetPinCatalogueQuery(),
            ct
        );

        await this.SendResultAsync(result, ct);
    }
}
