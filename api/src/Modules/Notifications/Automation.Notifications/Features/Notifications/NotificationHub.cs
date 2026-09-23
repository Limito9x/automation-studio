using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Automation.Notifications.Features.Notifications;

[Authorize]
public class NotificationHub : Hub
{
    // Hub methods can be added here if clients need to invoke methods on the server.
    // For now, it's used only for server-to-client push notifications.
}



