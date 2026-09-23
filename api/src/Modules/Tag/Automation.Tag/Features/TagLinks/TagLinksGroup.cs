namespace Automation.Tag.Features.TagLinks;

public class TagLinksGroup : Group
{
    public TagLinksGroup()
    {
        Configure("tag-links", ep =>
        {
            ep.Description(x => x.WithTags("Tags"));
        });
    }
}
