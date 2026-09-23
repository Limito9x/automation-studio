namespace Automation.Pipeline.Features.Nodes;

public sealed class NodesGroup : Group
{
    public NodesGroup()
    {
        Configure("pipeline/nodes", ep =>
        {
            ep.Description(x => x.WithTags("Pipeline Nodes"));
        });
    }
}
