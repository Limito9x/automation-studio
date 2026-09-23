namespace Automation.Identity.Features.Roles.UpdateRolePermissions;

public record UpdateRolePermissionsCommand(Guid Id, List<string> Permissions);



