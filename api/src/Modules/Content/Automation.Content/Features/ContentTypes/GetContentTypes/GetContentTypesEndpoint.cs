using Automation.Content.Constants;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.GetContentTypes;

public class GetContentTypesEndpoint(IMessageBus bus)
    : Endpoint<GetContentTypesQuery, PagedResult<ContentTypeDto>>
{
    public override void Configure()
    {
        Get(ContentRoutes.NestedContentTypes);
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.GetAll);
        Description(x => x.WithName("GetContentTypes"));
    }

    public override async Task HandleAsync(
        GetContentTypesQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<ContentTypeDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

