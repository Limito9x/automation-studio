using Automation.Content.Constants;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.GetContentType;

public class GetContentTypeEndpoint(IMessageBus bus)
    : Endpoint<GetContentTypeQuery, ContentTypeDto>
{
    public override void Configure()
    {
        Get(ContentRoutes.NestedContentTypes + "/{key}");
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.GetById);
        Description(x => x.WithName("GetContentType"));
    }

    public override async Task HandleAsync(
        GetContentTypeQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentTypeDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

