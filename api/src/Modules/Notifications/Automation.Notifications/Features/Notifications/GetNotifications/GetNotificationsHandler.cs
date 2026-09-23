using Automation.Notifications.Infrastructure.Persistence;
using Automation.Notifications.Shared.Dtos;
using Automation.SharedKernel.Infrastructure.Cursor;
using Automation.SharedKernel.Abstractions.Cursor;
using Microsoft.EntityFrameworkCore;

namespace Automation.Notifications.Features.Notifications.GetNotifications;

public class GetNotificationsHandler(ICurrentUserProvider userProvider, NotificationsDbContext dbContext)
{
    public async Task<Result<CursorPage<NotificationDto>>> HandleAsync(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = userProvider.UserId;
        if (currentUserId == null)
        {
            return Result.Fail("User not found");
        }

        var cursorPageNotifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && !x.ArchivedAt.HasValue)
            .ToCursorPageAsync(
                n => n.CreatedAt,
                n => n.Id,
                n => n.Adapt<NotificationDto>(),
                request,
                cancellationToken);

        return Result.Ok(cursorPageNotifications);
    }
}



