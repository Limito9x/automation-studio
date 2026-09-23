namespace Automation.Notifications.Features.Notifications.GetUnreadCount;

public class GetUnreadCountEndpoint(IMessageBus bus) : EndpointWithoutRequest<int>
{
    public override void Configure()
    {
        Get("/unread-count");
        Group<NotificationsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<int>>(new GetUnreadCountQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}



