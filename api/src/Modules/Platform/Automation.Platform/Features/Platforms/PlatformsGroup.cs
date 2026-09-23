namespace Automation.Platform.Features.Platforms;

public class PlatformsGroup : Group
{
    public PlatformsGroup()
    {
        Configure("platforms", ep =>
        {
            ep.Description(x => x
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .WithTags("Platforms"));
        });
    }
}

