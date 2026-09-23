using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.UpdateTag;

public class UpdateTagEndpoint(IMessageBus bus) : Endpoint<UpdateTagCommand, TagItemDto>
{
    public override void Configure()
    {
        Put("{id:guid}");
        Group<TagsGroup>();
        Permissions(P.Tag.Update);
    }

    public override async Task HandleAsync(UpdateTagCommand command, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<TagItemDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}