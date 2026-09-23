using Automation.Content.Constants;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.DeleteContentItem;

public class DeleteContentItemEndpoint(IMessageBus bus)
    : Endpoint<DeleteContentItemCommand>
{
    public override void Configure()
    {
        Delete(ContentRoutes.ContentItem);
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.Delete);
        Description(x => x.WithName("DeleteContentItem"));
    }

    public override async Task HandleAsync(
        DeleteContentItemCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

