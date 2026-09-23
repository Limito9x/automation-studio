using Automation.Content.Shared.Dtos;
using Automation.Content.Constants;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

public class UpdateContentItemEndpoint(IMessageBus bus)
    : Endpoint<UpdateContentItemCommand, ContentItemDto>
{
    public override void Configure()
    {
        Put(ContentRoutes.ContentItem);
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.Update);
        Description(x => x.WithName("UpdateContentItem"));
    }

    public override async Task HandleAsync(
        UpdateContentItemCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentItemDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

