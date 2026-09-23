namespace Automation.Notifications.Features.Notifications.Archive;

public record ArchiveCommand(Guid Id)
{
    public Guid UserId { get; set; }
}



