using Automation.SharedKernel.Extensions.Results;
using FastEndpoints;
using Wolverine;

namespace Automation.Files.Features.Assets.RequestUpload;

public class RequestUploadEndpoint(IMessageBus bus) : Endpoint<RequestUploadCommand, IReadOnlyList<AssetUploadDto>>
{
    public override void Configure()
    {
        Post("request-upload");
        Group<AssetsGroup>();
        AllowAnonymous(); // Depending on auth setup
    }

    public override async Task HandleAsync(RequestUploadCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<FluentResults.Result<IReadOnlyList<AssetUploadDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



