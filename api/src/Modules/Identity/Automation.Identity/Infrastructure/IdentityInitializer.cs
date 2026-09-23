using Automation.Identity.Constants;
using Automation.Identity.Domain;
using Automation.SharedKernel.Abstractions.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Automation.Identity.Infrastructure;

public class IdentityInitializer(
    IServiceProvider serviceProvider,
    ILogger<IdentityInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var permissionRegistry = scope.ServiceProvider.GetRequiredService<GlobalPermissionRegistry>();

        await SeedRolesAsync(roleManager);
        await SeedPermissionsForSuperAdminAsync(roleManager, permissionRegistry);
        await SeedDefaultUserAsync(userManager);
    }

    private async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        foreach (var roleName in IdentityRoles.DefaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Creating role {RoleName}", roleName);
                await roleManager.CreateAsync(new Role { Name = roleName });
            }
        }
    }

    private async Task SeedPermissionsForSuperAdminAsync(RoleManager<Role> roleManager, GlobalPermissionRegistry registry)
    {
        var superAdminRole = await roleManager.FindByNameAsync(IdentityRoles.SuperAdmin);
        if (superAdminRole is null) return;

        var existingClaims = await roleManager.GetClaimsAsync(superAdminRole);
        var existingPermissions = existingClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToHashSet();

        var allPermissions = registry.Modules
            .SelectMany(m => m.Value.Values)
            .SelectMany(p => p)
            .Distinct()
            .ToList();

        var newPermissions = allPermissions.Where(p => !existingPermissions.Contains(p)).ToList();

        foreach (var permission in newPermissions)
        {
            logger.LogInformation("Granting permission {Permission} to {Role}", permission, IdentityRoles.SuperAdmin);
            await roleManager.AddClaimAsync(superAdminRole, new Claim("Permission", permission));
        }
    }

    private async Task SeedDefaultUserAsync(UserManager<User> userManager)
    {
        var defaultEmail = "admin@local.com";
        var defaultPassword = "Admin@123!";

        var user = await userManager.FindByEmailAsync(defaultEmail);
        if (user is null)
        {
            logger.LogInformation("Creating default super admin user.");
            user = new User
            {
                UserName = defaultEmail,
                Email = defaultEmail,
                FirstName = "Super",
                LastName = "Admin",
                DisplayName = "Super Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, defaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, IdentityRoles.SuperAdmin);
            }
            else
            {
                logger.LogError("Failed to create default user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure the user is in the SuperAdmin role
            if (!await userManager.IsInRoleAsync(user, IdentityRoles.SuperAdmin))
            {
                await userManager.AddToRoleAsync(user, IdentityRoles.SuperAdmin);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}



