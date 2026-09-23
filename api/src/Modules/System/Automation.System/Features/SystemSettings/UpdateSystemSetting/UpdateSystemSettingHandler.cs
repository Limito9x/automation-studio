using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.SystemSettings.UpdateSystemSetting;

public class UpdateSystemSettingHandler(SystemDbContext db)
{
    public async Task<Result<SystemSettingDto>> HandleAsync(
        UpdateSystemSettingCommand request,
        CancellationToken cancellationToken)
    {
        _ = db;
        // TODO: Implement logic here
        
        return Result.Ok(new SystemSettingDto(request.Id, "Default", "Default", "Default", null));
    }
}



