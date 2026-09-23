using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.UpdateContentType;

public class UpdateContentTypeEndpoint(IMessageBus bus)
    : Endpoint<UpdateContentTypeCommand, ContentTypeDto>
{
    public override void Configure()
    {
        Put("{Id:guid}");
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.Update);
        Description(x => x.WithName("UpdateContentType"));
    }

    public override async Task HandleAsync(
        UpdateContentTypeCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentTypeDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

