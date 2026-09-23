namespace Automation.SharedKernel.Domain.Events;

public record AuditLogCreatedMessage(
    string? UserId,
    string Action,
    string EntityName,
    string EntityId,
    string? OldValues, // Will be JSON string
    string? NewValues, // Will be JSON string
    DateTimeOffset Timestamp,
    string? IpAddress,
    string? UserAgent
);



