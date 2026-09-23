using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Auth.Login;

public class LoginEndpoint(IMessageBus bus) : Endpoint<LoginCommand, LoginResult>
{
    public override void Configure()
    {
        Post("/login");
        Group<AuthGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginCommand req, CancellationToken ct)
    {
        req.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        req.UserAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var result = await bus.InvokeAsync<Result<LoginResult>>(req, ct);
        
        if (result.IsSuccess)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Value.RefreshTokenExpiry
            };
            HttpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
        }

        await this.SendResultAsync(result, ct);
    }
}



