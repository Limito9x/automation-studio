using Automation.SharedKernel.Infrastructure.Persistence;
using Automation.Identity.Infrastructure.Auth;

namespace Automation.Identity.Features.Auth.GetPermissions;

public class GetPermissionsHandler(
    IPermissionService permissionService,
    ICurrentUserProvider userProvider)
{
    public async Task<Result<List<string>>> HandleAsync(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = userProvider.UserId;
        if (currentUserId == null)
        {
            return Result.Fail("User not found");
        }

        var permissions = await permissionService.GetPermissionsAsync(currentUserId.Value, cancellationToken);
        return Result.Ok(permissions);
    }
}



