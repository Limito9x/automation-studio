using Automation.Notifications.Shared.Dtos;

namespace Automation.Notifications.Features.Notifications.Archive;

public class ArchiveEndpoint(IMessageBus bus) : Endpoint<ArchiveCommand, Result>
{
    public override void Configure()
    {
        Put("/{id}/archive");
        Group<NotificationsGroup>();
    }

    public override async Task HandleAsync(ArchiveCommand req, CancellationToken ct)
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



