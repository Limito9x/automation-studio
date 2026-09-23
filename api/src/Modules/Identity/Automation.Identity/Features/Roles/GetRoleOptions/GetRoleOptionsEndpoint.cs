using Automation.Identity.Shared.Dtos;

namespace Automation.Identity.Features.Roles.GetRoleOptions;

public class GetRoleOptionsEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<List<RoleDto>>
{
    public override void Configure()
    {
        Get("/options");
        Group<RolesGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<RoleDto>>>(new GetRoleOptionsQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}



