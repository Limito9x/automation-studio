using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Users;

public class UsersGroup : Group
{
    public UsersGroup()
    {
        Configure("users", ep =>
        {
            ep.Description(x => x
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .WithTags("Users"));
        });
    }
}



