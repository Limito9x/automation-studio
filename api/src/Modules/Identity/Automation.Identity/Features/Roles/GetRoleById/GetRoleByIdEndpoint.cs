using Automation.Identity.Shared.Dtos;

namespace Automation.Identity.Features.Roles.GetRoleById;

public class GetRoleByIdEndpoint(IMessageBus bus)
    : Endpoint<GetRoleByIdQuery, RoleDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<RolesGroup>();
        Permissions(P.Roles.GetById);
    }

    public override async Task HandleAsync(
        GetRoleByIdQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<RoleDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



