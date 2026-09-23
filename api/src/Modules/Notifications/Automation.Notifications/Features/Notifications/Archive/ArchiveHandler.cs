using Automation.Notifications.Domain;
using Automation.Notifications.Infrastructure.Persistence;
using Automation.Notifications.Shared.Dtos;

using Microsoft.EntityFrameworkCore;

namespace Automation.Notifications.Features.Notifications.Archive;

public class ArchiveHandler(NotificationsDbContext dbContext)
{
    public async Task<Result> HandleAsync(
        ArchiveCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);
            
        if (notification is null)
            return Result.Fail(new Error("Notification not found").WithMetadata("StatusCode", 404));

        notification.ArchivedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}



