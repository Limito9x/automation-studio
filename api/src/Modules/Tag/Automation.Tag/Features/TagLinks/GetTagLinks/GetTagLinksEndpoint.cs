using Automation.Tag.Contracts.Dtos;

namespace Automation.Tag.Features.TagLinks.GetTagLinks;

public class GetTagLinksEndpoint(IMessageBus bus) : Endpoint<GetTagLinksQuery, IReadOnlyList<TagLinkDetailDto>>
{
    public override void Configure()
    {
        Get("");
        Group<TagLinksGroup>();
        Permissions(P.TagLink.GetAll);
    }

    public override async Task HandleAsync(GetTagLinksQuery query, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<TagLinkDetailDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
