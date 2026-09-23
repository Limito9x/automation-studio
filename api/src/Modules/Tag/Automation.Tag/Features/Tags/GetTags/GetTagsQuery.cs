namespace Automation.Tag.Features.Tags.GetTags;

public record GetTagsQuery(Guid? ProjectId = null, string? RootPath = null, string? Search = null);