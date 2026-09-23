using FastEndpoints;

namespace Automation.Content.Features.ContentItems;

public sealed class ContentItemsGroup : Group
{
    public ContentItemsGroup()
    {
        Configure("", ep =>
        {
            ep.Description(b => b.WithTags("ContentItems"));
        });
    }
}

