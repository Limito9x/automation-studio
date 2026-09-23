namespace Automation.Tag.Features.Tags;

public class TagsGroup : Group
{
    public TagsGroup()
    {
        Configure("tags", ep =>
        {
            ep.Description(x => x.WithTags("Tags"));
        });
    }
}
