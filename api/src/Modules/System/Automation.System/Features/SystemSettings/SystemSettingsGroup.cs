using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Automation.SystemModule.Features.SystemSettings;

public sealed class SystemSettingsGroup : Group
{
    public SystemSettingsGroup()
    {
        Configure("/systemsettings", ep =>
        {
            ep.Description(b => b.WithTags("SystemSettings"));
        });
    }
}



