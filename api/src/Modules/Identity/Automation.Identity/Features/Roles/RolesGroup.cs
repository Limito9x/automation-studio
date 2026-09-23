using Microsoft.AspNetCore.Http;
using FastEndpoints;

namespace Automation.Identity.Features.Roles;

public sealed class RolesGroup : Group
{
    public RolesGroup()
    {
        Configure("roles", ep =>
        {
            ep.Description(x => x
                .WithTags("Roles"));
        });
    }
}




