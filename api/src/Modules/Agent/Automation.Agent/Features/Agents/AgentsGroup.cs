namespace Automation.Agent.Features.Agents;

public class AgentsGroup : Group
{
    public AgentsGroup()
    {
        Configure("agents", ep =>
        {
            ep.Description(x => x.WithTags("Agents"));
        });
    }
}

