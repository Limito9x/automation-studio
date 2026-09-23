namespace Automation.SystemModule.Features.AuditLogs;

public class AuditLogsGroup : Group
{
    public AuditLogsGroup()
    {
        Configure("audit-logs", ep =>
        {
            ep.Description(x => x.WithTags("AuditLogs"));
        });
    }
}



