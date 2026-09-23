using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Identity.Constants;

public class IdentityPermissions
{
    public static UsersFeature Users { get; } = new();
    public static RolesFeature Roles { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Users", Users.All },
        { "Roles", Roles.All }
    };

    public class UsersFeature() : BaseCrudPermission("users") 
    { 
        public const string Export = "users:export";
        public override IReadOnlyList<string> All => [.. base.All, Export];
    }
    
    public class RolesFeature() : BaseCrudPermission("roles") 
    { 
        public const string Assign = "roles:assign";
        public override IReadOnlyList<string> All => [.. base.All, Assign];
    }
}



