using Automation.Notifications.Infrastructure.Persistence;
using Automation.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace Automation.Notifications.Features.Notifications.GetUnreadCount;

public class GetUnreadCountHandler(ICurrentUserProvider userProvider, NotificationsDbContext dbContext)
{
    public async Task<Result<int>> HandleAsync(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userProvider.UserId;

        if(userId == null) 
        {
            return Result.Fail(new NotFoundError("User not found!"));
        }

        var count = await dbContext.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead && !x.ArchivedAt.HasValue, cancellationToken);
            
        return Result.Ok(count);
    }
}



