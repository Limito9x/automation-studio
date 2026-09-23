namespace Automation.Content.Features.ContentTypes;

public sealed class ContentTypesGroup : Group
{
    public ContentTypesGroup()
    {
        Configure("", ep =>
        {
            ep.Description(b => b.WithTags("ContentTypes"));
        });
    }
}

