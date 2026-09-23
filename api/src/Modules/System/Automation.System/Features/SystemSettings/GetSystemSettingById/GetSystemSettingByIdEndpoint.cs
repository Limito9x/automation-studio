using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.SystemSettings.GetSystemSettingById;

public class GetSystemSettingByIdEndpoint(IMessageBus bus)
    : Endpoint<GetSystemSettingByIdQuery, SystemSettingDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<SystemSettingsGroup>();
        Permissions(P.SystemSettings.GetById);
        Description(x => x.WithName("GetSystemSettingById"));
    }

    public override async Task HandleAsync(
        GetSystemSettingByIdQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<SystemSettingDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



