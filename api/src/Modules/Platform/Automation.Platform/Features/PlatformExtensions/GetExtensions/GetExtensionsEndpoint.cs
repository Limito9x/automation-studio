using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.PlatformExtensions.GetExtensions;

public class GetExtensionsEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<PlatformExtensionDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<PlatformExtensionsGroup>();
        Permissions(P.PlatformExtension.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<PlatformExtensionDto>>>(new GetExtensionsQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}

