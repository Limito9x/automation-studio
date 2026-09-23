using Automation.Content.Constants;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.LookupContentItems;

public class LookupContentItemsEndpoint(IMessageBus bus)
    : Endpoint<LookupContentItemsQuery, List<ContentLookupDto>>
{
    public override void Configure()
    {
        Get(ContentRoutes.ContentItemsLookup);
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.GetAll);
        Description(x => x.WithName("LookupContentItems"));
    }

    public override async Task HandleAsync(
        LookupContentItemsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<ContentLookupDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
