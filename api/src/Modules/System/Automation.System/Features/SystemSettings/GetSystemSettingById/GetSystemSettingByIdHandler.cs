using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.SystemSettings.GetSystemSettingById;

public class GetSystemSettingByIdHandler(SystemDbContext db)
{
    public async Task<Result<SystemSettingDto>> HandleAsync(
        GetSystemSettingByIdQuery query,
        CancellationToken ct)
    {
        _ = db;
        // TODO: Implement get by id logic here
        
        return Result.Ok(new SystemSettingDto(query.Id, "Default", "Default", "Default", null));
    }
}



