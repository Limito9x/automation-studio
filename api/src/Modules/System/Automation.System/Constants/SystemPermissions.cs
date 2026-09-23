using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.SystemModule.Constants;

public class SystemPermissions
{
    public static AuditLogFeature AuditLogs { get; } = new();
    public static SystemSettingsFeature SystemSettings { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "AuditLogs", AuditLogs.All },
        { "SystemSettings", SystemSettings.All }
    };

    public class AuditLogFeature() : BasePermission("auditlogs")
    {
        public string GetAll => $"{Feature}:get-all";
        public string GetById => $"{Feature}:get-by-id";
        
        public IReadOnlyList<string> All => [GetAll, GetById];
    }
    
    public class SystemSettingsFeature() : BasePermission("systemsettings")
    {
        public string GetAll => $"{Feature}:get-all";
        public string GetById => $"{Feature}:get-by-id";
        public string Update => $"{Feature}:update";
        
        public IReadOnlyList<string> All => [GetAll, GetById, Update];
    }
}



