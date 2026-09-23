using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Automation.SystemModule.Shared.Dtos;
using Gridify;

namespace Automation.SystemModule.Features.SystemSettings.GetSystemSettings;

public class GetSystemSettingsHandler(SystemDbContext db)
{
    public async Task<Result<PagedResult<SystemSettingDto>>> HandleAsync(
        GetSystemSettingsQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<SystemSetting>()
            .GenerateMappings();

        var result = await db.Set<SystemSetting>()
            .ToPagedResultAsync<SystemSetting, SystemSettingDto>(query, mapper, ct);
            
        return result;
    }
}



