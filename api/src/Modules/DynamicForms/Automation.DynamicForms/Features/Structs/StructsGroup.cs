namespace Automation.DynamicForms.Features.Structs;

public sealed class StructsGroup : Group
{
    public StructsGroup()
    {
        Configure("projects/{projectId:guid}/structs", ep =>
        {
            ep.Description(b => b.WithTags("Structs"));
        });
    }
}
