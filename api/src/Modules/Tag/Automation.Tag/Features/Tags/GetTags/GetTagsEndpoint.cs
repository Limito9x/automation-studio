using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.GetTags;

public class GetTagsEndpoint(IMessageBus bus) : Endpoint<GetTagsQuery, IReadOnlyList<TagItemDto>>
{
    public override void Configure()
    {
        Get("");
        Group<TagsGroup>();
        Permissions(P.Tag.GetAll);
    }

    public override async Task HandleAsync(GetTagsQuery query, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<TagItemDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}