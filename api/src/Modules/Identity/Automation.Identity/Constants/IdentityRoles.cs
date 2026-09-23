namespace Automation.Identity.Constants;

public static class IdentityRoles
{
    public const string SuperAdmin = "super-admin";
    public const string Admin = "admin";
    public const string User = "user";
    
    public static IReadOnlyList<string> DefaultRoles => [SuperAdmin, Admin, User];
}



