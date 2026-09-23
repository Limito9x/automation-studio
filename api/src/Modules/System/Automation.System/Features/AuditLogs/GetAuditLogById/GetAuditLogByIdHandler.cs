using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Automation.SystemModule.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.SystemModule.Features.AuditLogs.GetAuditLogById;

public class GetAuditLogByIdHandler(SystemDbContext db)
{
    public async Task<Result<AuditLogDto>> HandleAsync(
        GetAuditLogByIdQuery query,
        CancellationToken ct)
    {
        var log = await db.Set<AuditLog>()
            .Where(x => x.Id == query.Id)
            .Select(x => new AuditLogDto(
                x.Id,
                x.UserId,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.OldValues,
                x.NewValues,
                x.Timestamp,
                x.IpAddress,
                x.UserAgent))
            .FirstOrDefaultAsync(ct);

        if (log is null)
            return Result.Fail(new Error("Audit log not found").WithMetadata("Code", "404"));

        return log;
    }
}



