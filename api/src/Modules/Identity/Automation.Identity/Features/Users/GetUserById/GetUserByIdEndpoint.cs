namespace Automation.Identity.Features.Users.GetUserById;

public class GetUserByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<UserDto>
{
    public override void Configure()
    {
        Get("/{Id}");
        Group<UsersGroup>();
        Permissions(P.Users.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var result = await bus.InvokeAsync<Result<UserDto>>(new GetUserByIdQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}



