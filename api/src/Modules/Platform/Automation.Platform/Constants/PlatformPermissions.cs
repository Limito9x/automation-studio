using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Platform.Constants;

public class PlatformPermissions
{
    public static PlatformFeature Platform { get; } = new();
    public static PlatformExtensionFeature PlatformExtension { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Platform", Platform.All },
        { "PlatformExtension", PlatformExtension.All }
    };

    public class PlatformFeature() : BaseCrudPermission("platform") { }
    public class PlatformExtensionFeature() : BaseCrudPermission("platform_extension") { }
}

