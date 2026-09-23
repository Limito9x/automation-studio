using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.SystemSettings.UpdateSystemSetting;

public class UpdateSystemSettingEndpoint(IMessageBus bus)
    : Endpoint<UpdateSystemSettingCommand, SystemSettingDto>
{
    public override void Configure()
    {
        Put("/{id}");
        Group<SystemSettingsGroup>();
        Permissions(P.SystemSettings.Update);
        Description(x => x.WithName("UpdateSystemSetting"));
    }

    public override async Task HandleAsync(
        UpdateSystemSettingCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<SystemSettingDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



