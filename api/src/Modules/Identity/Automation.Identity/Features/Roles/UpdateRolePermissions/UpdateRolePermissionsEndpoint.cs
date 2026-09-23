namespace Automation.Identity.Features.Roles.UpdateRolePermissions;

public class UpdateRolePermissionsEndpoint(IMessageBus bus)
    : Endpoint<UpdateRolePermissionsCommand>
{
    public override void Configure()
    {
        Put("/{id}/permissions");
        Group<RolesGroup>();
        Permissions(P.RolesFeature.Assign);
    }

    public override async Task HandleAsync(
        UpdateRolePermissionsCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}




