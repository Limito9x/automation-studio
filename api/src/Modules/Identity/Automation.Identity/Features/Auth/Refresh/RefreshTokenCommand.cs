using System.Text.Json.Serialization;

namespace Automation.Identity.Features.Auth.Refresh;

public class RefreshTokenCommand
{
    [JsonIgnore]
    public string Token { get; set; } = string.Empty;

    [JsonIgnore]
    public string? IpAddress { get; set; }

    [JsonIgnore]
    public string? UserAgent { get; set; }
}

public record RefreshTokenResult(
    string AccessToken,
    [property: JsonIgnore] string NewRefreshToken,
    [property: JsonIgnore] DateTime RefreshTokenExpiry
);



