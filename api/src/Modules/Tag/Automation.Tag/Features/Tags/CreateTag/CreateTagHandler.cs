using Automation.Tag.Contracts;
using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Wolverine.Attributes;

namespace Automation.Tag.Features.Tags.CreateTag;

[Transactional(typeof(TagDbContext))]
public class CreateTagHandler(ITagApi tagApi)
{
    public async Task<Result<TagItemDto>> HandleAsync(CreateTagCommand command, CancellationToken ct)
    {
        var result = await tagApi.EnsureTagAsync(
            command.ProjectId,
            command.Path,
            command.Color,
            command.Description,
            ct
        );

        if (result.IsFailed)
            return Result.Fail<TagItemDto>(result.Errors);

        var dto = result.Value;
        return Result.Ok(new TagItemDto(
            dto.Id,
            dto.ProjectId,
            dto.Path,
            dto.Name,
            dto.Color,
            dto.Description,
            null,
            dto.CreatedAt
        ));
    }
}