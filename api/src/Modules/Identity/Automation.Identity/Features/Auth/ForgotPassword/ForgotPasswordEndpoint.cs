using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Auth.ForgotPassword;

public class ForgotPasswordEndpoint(IMessageBus bus) : Endpoint<ForgotPasswordCommand, string>
{
    public override void Configure()
    {
        Post("/forgot-password");
        Group<AuthGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(ForgotPasswordCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<string>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



