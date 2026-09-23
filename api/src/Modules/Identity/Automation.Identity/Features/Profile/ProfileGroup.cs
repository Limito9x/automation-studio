using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Profile;

public sealed class ProfileGroup : Group
{
    public ProfileGroup()
    {
        Configure("profile", ep =>
        {
            ep.Description(x => x.WithTags("Profile"));
        });
    }
}



