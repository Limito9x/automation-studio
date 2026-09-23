namespace Automation.Platform.Features.PlatformExtensions;

public class PlatformExtensionsGroup : Group
{
    public PlatformExtensionsGroup()
    {
        Configure("platform-extensions", ep =>
        {
            ep.Description(x => x
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .WithTags("PlatformExtensions"));
        });
    }
}

