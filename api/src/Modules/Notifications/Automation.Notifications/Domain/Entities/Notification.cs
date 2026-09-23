using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Automation.Notifications.Domain.Enums;
using Automation.SharedKernel.Domain.Entities;

namespace Automation.Notifications.Domain.Entities;

public class Notification : BaseEntity
{

    public Guid UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the notification for business logic (e.g. "OrderCreated", "CommentReply")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

    public JsonDocument? Data { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    [NotMapped]
    public bool IsArchived => ArchivedAt.HasValue;
}



