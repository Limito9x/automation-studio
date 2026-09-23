using Automation.Identity.Shared.Dtos;

namespace Automation.Identity.Features.Roles.DeleteRole;

public class DeleteRoleEndpoint(IMessageBus bus)
    : Endpoint<DeleteRoleCommand>
{
    public override void Configure()
    {
        Delete("/{id}");
        Group<RolesGroup>();
        Permissions(P.Roles.Delete);
    }

    public override async Task HandleAsync(
        DeleteRoleCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



