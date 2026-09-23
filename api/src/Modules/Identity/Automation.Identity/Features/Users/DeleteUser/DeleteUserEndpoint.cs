namespace Automation.Identity.Features.Users.DeleteUser;

public class DeleteUserEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{Id}");
        Group<UsersGroup>();
        Permissions(P.Users.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var result = await bus.InvokeAsync<Result<string>>(new DeleteUserCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}



