

namespace Automation.Identity.Features.Auth.Register;

public class RegisterEndpoint(IMessageBus bus)
    : Endpoint<RegisterCommand, Result>
{
    public override void Configure()
    {
        Post("/register");
        Group<AuthGroup>();
        AllowAnonymous();
        
    }

    public override async Task HandleAsync(
        RegisterCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



