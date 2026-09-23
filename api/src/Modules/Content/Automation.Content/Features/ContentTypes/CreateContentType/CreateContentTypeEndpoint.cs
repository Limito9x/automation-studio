using Automation.Content.Constants;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

public class CreateContentTypeEndpoint(IMessageBus bus)
    : Endpoint<CreateContentTypeCommand, ContentTypeDto>
{
    public override void Configure()
    {
        Post(ContentRoutes.NestedContentTypes);
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.Create);
        Description(x => x.WithName("CreateContentType"));
    }

    public override async Task HandleAsync(
        CreateContentTypeCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentTypeDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

