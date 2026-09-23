using Automation.Tag.Contracts;
using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Wolverine.Attributes;

namespace Automation.Tag.Features.TagLinks.CreateTagLink;

[Transactional(typeof(TagDbContext))]
public class CreateTagLinkHandler(ITagApi tagApi)
{
    public async Task<Result<TagLinkDto>> HandleAsync(
        CreateTagLinkCommand command,
        CancellationToken ct
    )
    {
        var result = await tagApi.AssignTagAsync(
            command.ProjectId,
            command.EntityType,
            command.EntityId,
            command.TagPath,
            command.TargetSubPath,
            command.Metadata?.RootElement.ToString(),
            ct
        );

        if (result.IsFailed)
            return Result.Fail<TagLinkDto>(result.Errors);

        var l = result.Value;
        return Result.Ok(new TagLinkDto(
            l.Id,
            l.ProjectId,
            l.TagId,
            l.TagPath,
            l.EntityType,
            l.EntityId,
            l.TargetSubPath,
            l.MetadataJson
        ));
    }
}
