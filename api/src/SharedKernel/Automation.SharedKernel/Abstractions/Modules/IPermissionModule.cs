namespace Automation.SharedKernel.Abstractions.Modules;

public interface IPermissionModule
{
    Dictionary<string, IReadOnlyList<string>> GetPermissions();
}



