#if DEBUG
using System.Security.Claims;
using Automation.Notifications.Contracts.Messages;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace Automation.Api.Dev;

public sealed class TestNotificationEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/test-notification");
        Group<DevGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        await bus.PublishAsync(new SystemNotificationCreatedMessage(
            UserId: userId,
            Title: "?? Test Notification",
            Message: $"Dev endpoint fired at {DateTimeOffset.UtcNow:HH:mm:ss}",
            Type: "DevTest",
            Severity: "Info"
        ));

        await HttpContext.Response.WriteAsJsonAsync(new { Message = "Notification sent successfully" }, ct);
    }
}
#endif



