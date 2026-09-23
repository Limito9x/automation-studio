using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.DynamicForms.Constants;

public class DynamicFormsPermissions
{
    public static StructFeature Structs { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Structs", Structs.All }
    };

    public class StructFeature() : BaseCrudPermission("structs") { }
}
