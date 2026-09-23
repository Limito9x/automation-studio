using System.Text.Json;
using Automation.Notifications.Domain.Enums;

namespace Automation.Notifications.Shared.Dtos;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    NotificationSeverity Severity,
    JsonDocument? Data,
    bool IsRead,
    DateTimeOffset CreatedAt
);



