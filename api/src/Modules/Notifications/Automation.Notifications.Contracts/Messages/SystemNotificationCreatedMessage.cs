using System;

namespace Automation.Notifications.Contracts.Messages;

public record SystemNotificationCreatedMessage(
    Guid UserId,
    string Title,
    string Message,
    string Type,
    string Severity = "Info",
    string? DataJson = null
);



