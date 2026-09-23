namespace Automation.Identity.Features.Auth.Logout;

public class LogoutEndpoint(IMessageBus bus) : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Post("/logout");
        Group<AuthGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var req = new LogoutCommand();
        if (HttpContext.Request.Cookies.TryGetValue("refreshToken", out var token))
        {
            req.RefreshToken = token;
        }

        var result = await bus.InvokeAsync<Result<string>>(req, ct);
        
        HttpContext.Response.Cookies.Delete("refreshToken");

        await this.SendResultAsync(result, ct);
    }
}



