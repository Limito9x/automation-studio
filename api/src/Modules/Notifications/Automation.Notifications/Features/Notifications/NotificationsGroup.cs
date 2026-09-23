namespace Automation.Notifications.Features.Notifications;

public sealed class NotificationsGroup : Group
{
    public NotificationsGroup()
    {
        Configure("notifications", ep =>
        {
            ep.Description(b => b.WithTags("Notifications"));
        });
    }
}



