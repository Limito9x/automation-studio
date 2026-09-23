using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Automation.SystemModule.Shared.Dtos;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.SystemModule.Features.AuditLogs.GetAuditLogs;

public class GetAuditLogsHandler(SystemDbContext db)
{
    public async Task<Result<PagedResult<AuditLogDto>>> HandleAsync(
        GetAuditLogsQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<AuditLog>()
            .GenerateMappings();

        if (query.Sort is null || query.Sort.Count == 0)
        {
            query.Sort = new Dictionary<string, bool> { { "Timestamp", false } };
        }

        var result = await db.AuditLogs
            .AsNoTracking()
            .ToPagedResultAsync<AuditLog, AuditLogDto>(query, mapper, ct);
            
        return result;
    }
}



