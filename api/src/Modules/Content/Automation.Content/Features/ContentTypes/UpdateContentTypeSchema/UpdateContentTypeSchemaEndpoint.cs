using Automation.Content.Constants;

namespace Automation.Content.Features.ContentTypes.UpdateContentTypeSchema;

public class UpdateContentTypeSchemaEndpoint(IMessageBus bus)
    : Endpoint<UpdateContentTypeSchemaCommand>
{
    public override void Configure()
    {
        Put(ContentRoutes.ContentType + "/schema");
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.Update);
        Description(x => x.WithName("UpdateContentTypeSchema"));
    }

    public override async Task HandleAsync(
        UpdateContentTypeSchemaCommand req,
        CancellationToken ct)
    {
        req = req with { Id = Route<Guid>("Id") };
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

