using System.Text.Json.Serialization;

namespace Automation.Identity.Features.Profile.UpdateProfile;

public class UpdateProfileCommand
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}



