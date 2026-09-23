namespace Automation.Content.Contracts;

public record ContentSummaryDto(
    Guid Id,
    string Name,
    Guid ContentTypeId,
    string ContentTypeName,
    string? ContentTypeColor,
    string? ContentTypeIcon
);
