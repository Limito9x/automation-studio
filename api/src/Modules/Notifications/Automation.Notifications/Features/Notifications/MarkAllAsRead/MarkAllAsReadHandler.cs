using Automation.Notifications.Domain;
using Automation.Notifications.Infrastructure.Persistence;
using Automation.Notifications.Shared.Dtos;

using Microsoft.EntityFrameworkCore;

namespace Automation.Notifications.Features.Notifications.MarkAllAsRead;

public class MarkAllAsReadHandler(NotificationsDbContext dbContext)
{
    public async Task<Result> HandleAsync(
        MarkAllAsReadCommand request,
        CancellationToken cancellationToken)
    {
        await dbContext.Notifications
            .Where(x => x.UserId == request.UserId && !x.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
            
        return Result.Ok();
    }
}



