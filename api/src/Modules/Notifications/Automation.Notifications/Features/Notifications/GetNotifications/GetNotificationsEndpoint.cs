using Automation.Notifications.Shared.Dtos;
using Automation.SharedKernel.Abstractions.Cursor;

namespace Automation.Notifications.Features.Notifications.GetNotifications;

public class GetNotificationsEndpoint(IMessageBus bus) : Endpoint<GetNotificationsQuery, CursorPage<NotificationDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<NotificationsGroup>();
    }

    public override async Task HandleAsync(GetNotificationsQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<CursorPage<NotificationDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



