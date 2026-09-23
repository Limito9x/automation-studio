namespace Automation.Identity.Features.Users.UpdateUser;

public class UpdateUserEndpoint(IMessageBus bus) : Endpoint<UpdateUserCommand, string>
{
    public override void Configure()
    {
        Put("/{Id}");
        Group<UsersGroup>();
        Permissions(P.Users.Update);
    }

    public override async Task HandleAsync(UpdateUserCommand req, CancellationToken ct)
    {
        req.Id = Route<Guid>("Id");
        var result = await bus.InvokeAsync<Result<string>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



