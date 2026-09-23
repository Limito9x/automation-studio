using Automation.Notifications.Shared.Dtos;

namespace Automation.Notifications.Features.Notifications.MarkAllAsRead;

public class MarkAllAsReadEndpoint(IMessageBus bus) : Endpoint<MarkAllAsReadCommand, Result>
{
    public override void Configure()
    {
        Put("/mark-all-as-read");
        Group<NotificationsGroup>();
    }

    public override async Task HandleAsync(MarkAllAsReadCommand req, CancellationToken ct)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        req.UserId = userId;
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



