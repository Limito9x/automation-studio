namespace Automation.Identity.Infrastructure.Auth;

public record UserRoleCacheItem(Guid RoleId, string ConcurrencyStamp);

public interface IPermissionService
{
    Task<List<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsUserActiveAsync(Guid userId, CancellationToken ct = default);
    Task ClearRolePermissionsCacheAsync(Guid roleId, string stamp, CancellationToken ct = default);
    Task CacheRolePermissionsAsync(Guid roleId, string concurrencyStamp, List<string> permissions, CancellationToken ct = default);
    Task ClearUserStatusCacheAsync(Guid userId, CancellationToken ct = default);
    Task ClearUserRolesCacheAsync(Guid userId, CancellationToken ct = default);
}



