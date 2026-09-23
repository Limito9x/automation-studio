using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.GetTagTree;

public class GetTagTreeEndpoint(IMessageBus bus) : Endpoint<GetTagTreeQuery, IReadOnlyList<TagTreeNodeDto>>
{
    public override void Configure()
    {
        Get("tree");
        Group<TagsGroup>();
        Permissions(P.Tag.GetAll);
    }

    public override async Task HandleAsync(GetTagTreeQuery query, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<TagTreeNodeDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
