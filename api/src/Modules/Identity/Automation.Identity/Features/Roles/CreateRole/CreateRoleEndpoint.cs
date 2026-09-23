using Automation.Identity.Shared.Dtos;

namespace Automation.Identity.Features.Roles.CreateRole;

public class CreateRoleEndpoint(IMessageBus bus)
    : Endpoint<CreateRoleCommand, RoleDto>
{
    public override void Configure()
    {
        Post("/"); // Change this method/route accordingly
        Group<RolesGroup>();
        Permissions(P.Roles.Create);
    }

    public override async Task HandleAsync(
        CreateRoleCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<RoleDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



