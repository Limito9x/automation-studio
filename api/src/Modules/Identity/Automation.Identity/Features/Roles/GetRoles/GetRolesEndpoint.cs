namespace Automation.Identity.Features.Roles.GetRoles;

public class GetRolesEndpoint(IMessageBus bus)
    : Endpoint<GetRolesQuery, PagedResult<RoleDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<RolesGroup>();
        Permissions(P.Roles.GetAll);
        RequestBinder(new PagedQueryBinder<GetRolesQuery>());
    }

    public override async Task HandleAsync(
        GetRolesQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<RoleDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



