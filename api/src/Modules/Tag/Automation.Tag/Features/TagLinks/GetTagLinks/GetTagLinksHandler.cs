using Automation.Tag.Contracts;
using Automation.Tag.Contracts.Dtos;
using Wolverine.Attributes;

namespace Automation.Tag.Features.TagLinks.GetTagLinks;

[NonTransactional]
public class GetTagLinksHandler(ITagApi tagApi)
{
    public async Task<Result<IReadOnlyList<TagLinkDetailDto>>> HandleAsync(GetTagLinksQuery query, CancellationToken ct)
    {
        return await tagApi.GetTagsByEntityAsync(query.EntityType, query.EntityId, ct);
    }
}
