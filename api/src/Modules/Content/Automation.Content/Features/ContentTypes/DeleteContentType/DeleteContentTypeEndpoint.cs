using Automation.Content.Constants;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.DeleteContentType;

public class DeleteContentTypeEndpoint(IMessageBus bus)
    : Endpoint<DeleteContentTypeCommand>
{
    public override void Configure()
    {
        Delete(ContentRoutes.ContentType);
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.Delete);
        Description(x => x.WithName("DeleteContentType"));
    }

    public override async Task HandleAsync(
        DeleteContentTypeCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

