using System.Text.Json.Serialization;

namespace Automation.Identity.Features.Auth.Logout;

public class LogoutCommand
{
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}



