using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint(IMessageBus bus) : Endpoint<ResetPasswordCommand, string>
{
    public override void Configure()
    {
        Post("/reset-password");
        Group<AuthGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResetPasswordCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<string>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



