using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Automation.Files.Features.Assets;

public sealed class AssetsGroup : Group
{
    public AssetsGroup()
    {
        Configure("assets", ep =>
        {
            ep.Description(x => x.WithTags("Assets"));
        });
    }
}



