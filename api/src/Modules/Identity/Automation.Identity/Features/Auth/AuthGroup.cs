using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Auth;

public class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure("auth", ep =>
        {
            ep.Description(x => x.WithTags("Auth"));
        });
    }
}



