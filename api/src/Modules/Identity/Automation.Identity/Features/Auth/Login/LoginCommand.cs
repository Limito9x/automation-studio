using System.Text.Json.Serialization;

namespace Automation.Identity.Features.Auth.Login;

public class LoginCommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    [JsonIgnore]
    public string? IpAddress { get; set; }

    [JsonIgnore]
    public string? UserAgent { get; set; }
}

public record LoginResult(
    string AccessToken, 
    [property: JsonIgnore] string RefreshToken, 
    [property: JsonIgnore] DateTime RefreshTokenExpiry
);



