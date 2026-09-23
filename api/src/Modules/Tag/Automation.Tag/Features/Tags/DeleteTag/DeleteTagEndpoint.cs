namespace Automation.Tag.Features.Tags.DeleteTag;

public class DeleteTagEndpoint(IMessageBus bus) : Endpoint<DeleteTagCommand>
{
    public override void Configure()
    {
        Delete("{id:guid}");
        Group<TagsGroup>();
        Permissions(P.Tag.Delete);
    }

    public override async Task HandleAsync(DeleteTagCommand command, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}