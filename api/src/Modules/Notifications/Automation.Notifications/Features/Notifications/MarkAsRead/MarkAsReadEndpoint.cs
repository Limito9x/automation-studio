namespace Automation.Notifications.Features.Notifications.MarkAsRead;

public class MarkAsReadEndpoint(IMessageBus bus) : Endpoint<MarkAsReadCommand, Result>
{
    public override void Configure()
    {
        Patch("/mark-as-read");
        Group<NotificationsGroup>();
    }

    public override async Task HandleAsync(MarkAsReadCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



