namespace Automation.Identity.Features.Auth.GetPermissions;

public class GetPermissionsEndpoint(IMessageBus bus) : EndpointWithoutRequest<List<string>>
{
    public override void Configure()
    {
        Get("/permissions");
        Group<AuthGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<string>>>(new GetPermissionsQuery(), ct);
        
        await this.SendResultAsync(result, ct);
    }
}



