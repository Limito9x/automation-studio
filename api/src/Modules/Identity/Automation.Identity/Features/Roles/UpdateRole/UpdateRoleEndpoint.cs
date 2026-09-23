using Automation.Identity.Shared.Dtos;

namespace Automation.Identity.Features.Roles.UpdateRole;

public class UpdateRoleEndpoint(IMessageBus bus)
    : Endpoint<UpdateRoleCommand, RoleDto>
{
    public override void Configure()
    {
        Put("/{id}");
        Group<RolesGroup>();
        Permissions(P.Roles.Update);
    }

    public override async Task HandleAsync(
        UpdateRoleCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<RoleDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



