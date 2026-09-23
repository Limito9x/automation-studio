namespace Automation.Identity.Features.Users.GetUsers;

public class GetUsersEndpoint(IMessageBus bus) : Endpoint<GetUsersQuery, PagedResult<UserDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<UsersGroup>();
        Permissions(P.Users.GetAll);
        RequestBinder(new PagedQueryBinder<GetUsersQuery>());
    }

    public override async Task HandleAsync(GetUsersQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<UserDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



