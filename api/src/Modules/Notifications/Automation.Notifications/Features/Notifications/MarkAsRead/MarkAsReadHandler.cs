using Automation.Notifications.Infrastructure.Persistence;
using Automation.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace Automation.Notifications.Features.Notifications.MarkAsRead;

public class MarkAsReadHandler(ICurrentUserProvider userProvider, NotificationsDbContext dbContext)
{
    public async Task<Result> HandleAsync(
        MarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = userProvider.UserId;
        if (currentUserId == null)
        {
            return Result.Fail("User not found");
        }

        var updatedCount = await dbContext.Notifications.Where(x => request.Ids.Contains(x.Id) && x.UserId == currentUserId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
        
        if (updatedCount == 0)
            return Result.Fail(new NotFoundError("Notification not found"));
        
        return Result.Ok();
    }
}



