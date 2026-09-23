using Microsoft.AspNetCore.Http;

namespace Automation.Identity.Features.Auth.Refresh;

public class RefreshEndpoint(IMessageBus bus) : EndpointWithoutRequest<RefreshTokenResult>
{
    public override void Configure()
    {
        Post("/refresh");
        Group<AuthGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var req = new RefreshTokenCommand();
        if (HttpContext.Request.Cookies.TryGetValue("refreshToken", out var token))
        {
            req.Token = token;
        }
        req.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        req.UserAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await bus.InvokeAsync<Result<RefreshTokenResult>>(req, ct);
        
        if (result.IsSuccess)
        {
            HttpContext.Response.Cookies.Append("refreshToken", result.Value.NewRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Value.RefreshTokenExpiry
            });
        }

        await this.SendResultAsync(result, ct);
    }
}



