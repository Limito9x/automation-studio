namespace Automation.Tag.Shared.Dtos;

public record TagItemDto(
    Guid Id,
    Guid ProjectId,
    string Path,
    string Name,
    string? Color,
    string? Description,
    Guid? ParentId,
    DateTimeOffset CreatedAt
);

public record TagLinkDto(
    Guid Id,
    Guid ProjectId,
    Guid TagId,
    string TagPath,
    string EntityType,
    Guid EntityId,
    string? TargetSubPath,
    string? MetadataJson
);

public record TagTreeNodeDto(
    Guid Id,
    string Path,
    string Name,
    string? Color,
    string? Description,
    List<TagTreeNodeDto> Children
);