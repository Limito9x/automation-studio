using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.TagLinks.CreateTagLink;

public class CreateTagLinkEndpoint(IMessageBus bus) : Endpoint<CreateTagLinkCommand, TagLinkDto>
{
    public override void Configure()
    {
        Post("");
        Group<TagLinksGroup>();
        Permissions(P.TagLink.Create);
    }

    public override async Task HandleAsync(CreateTagLinkCommand command, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<TagLinkDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}