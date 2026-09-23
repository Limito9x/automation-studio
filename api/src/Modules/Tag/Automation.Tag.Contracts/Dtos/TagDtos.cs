namespace Automation.Tag.Contracts.Dtos;

public record TagDto(
    Guid Id,
    Guid ProjectId,
    string Path,
    string Name,
    string? Color,
    string? Description,
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
    string? MetadataJson,
    TagDto? Tag = null
);

public record TagLinkDetailDto(
    Guid TagLinkId,
    Guid TagId,
    string TagPath,
    string TagName,
    string? TagColor,
    string? TagDescription,
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
