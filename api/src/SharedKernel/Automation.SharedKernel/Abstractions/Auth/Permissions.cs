namespace Automation.SharedKernel.Abstractions.Auth;

public abstract class BasePermission(string feature)
{
    protected readonly string Feature = feature.ToLower();
}

public abstract class BaseCrudPermission(string feature) : BasePermission(feature)
{
    public string GetAll => $"{Feature}:get-all";
    public string GetById => $"{Feature}:get-by-id";
    public string Create => $"{Feature}:create";
    public string Update => $"{Feature}:update";
    public string Delete => $"{Feature}:delete";

    public virtual IReadOnlyList<string> All => [GetAll, GetById, Create, Update, Delete];
}

public class GlobalPermissionRegistry
{
    // ModuleName -> FeatureName -> Permissions
    public Dictionary<string, Dictionary<string, IReadOnlyList<string>>> Modules { get; } = [];
}



