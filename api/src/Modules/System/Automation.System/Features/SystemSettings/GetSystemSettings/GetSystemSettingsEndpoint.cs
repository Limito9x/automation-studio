using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.SystemSettings.GetSystemSettings;

public class GetSystemSettingsEndpoint(IMessageBus bus)
    : Endpoint<GetSystemSettingsQuery, PagedResult<SystemSettingDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<SystemSettingsGroup>();
        Permissions(P.SystemSettings.GetAll);
        Description(x => x.WithName("GetSystemSettings"));
        RequestBinder(new PagedQueryBinder<GetSystemSettingsQuery>());
    }

    public override async Task HandleAsync(
        GetSystemSettingsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<SystemSettingDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



