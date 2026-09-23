#if DEBUG
using FastEndpoints;

namespace Automation.Api.Dev;

public sealed class DevGroup : Group
{
    public DevGroup()
    {
        Configure("dev", ep =>
        {
            ep.Description(b => b.WithTags("Dev"));
        });
    }
}
#endif



