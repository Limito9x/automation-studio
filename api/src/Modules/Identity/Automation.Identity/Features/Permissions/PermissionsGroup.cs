using Microsoft.AspNetCore.Http;
using FastEndpoints;

namespace Automation.Identity.Features.Permissions;

public sealed class PermissionsGroup : Group
{
    public PermissionsGroup()
    {
        Configure("permissions", ep =>
        {
            ep.Description(x => x
                .WithTags("Permissions"));
        });
    }
}




