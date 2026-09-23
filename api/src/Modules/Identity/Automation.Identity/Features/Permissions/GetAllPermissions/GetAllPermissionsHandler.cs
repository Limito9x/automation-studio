using Automation.SharedKernel.Abstractions.Auth;
using Wolverine;

namespace Automation.Identity.Features.Permissions.GetAllPermissions;

public class GetAllPermissionsHandler(GlobalPermissionRegistry registry)
{
    public Result<Dictionary<string, Dictionary<string, IReadOnlyList<string>>>> Handle(GetAllPermissionsQuery query)
    {
        return Result.Ok(registry.Modules);
    }
}





