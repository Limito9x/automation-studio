namespace Automation.Identity.Features.Permissions.GetAllPermissions;

public class GetAllPermissionsEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<Dictionary<string, Dictionary<string, IReadOnlyList<string>>>>
{
    public override void Configure()
    {
        Get("/");
        Group<PermissionsGroup>();
        Permissions(P.RolesFeature.Assign);
        
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<Dictionary<string, Dictionary<string, IReadOnlyList<string>>>>>(new GetAllPermissionsQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}




