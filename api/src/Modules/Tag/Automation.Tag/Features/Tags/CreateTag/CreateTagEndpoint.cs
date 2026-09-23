using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.CreateTag;

public class CreateTagEndpoint(IMessageBus bus) : Endpoint<CreateTagCommand, TagItemDto>
{
    public override void Configure()
    {
        Post("");
        Group<TagsGroup>();
        Permissions(P.Tag.Create);
    }

    public override async Task HandleAsync(CreateTagCommand command, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<TagItemDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}