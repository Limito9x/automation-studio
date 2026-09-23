namespace Automation.SystemModule.Shared.Dtos;

public record AuditLogDto(
    Guid Id, 
    string? UserId, 
    string Action, 
    string EntityName, 
    string EntityId, 
    string? OldValues, 
    string? NewValues, 
    DateTimeOffset Timestamp, 
    string? IpAddress, 
    string? UserAgent);



