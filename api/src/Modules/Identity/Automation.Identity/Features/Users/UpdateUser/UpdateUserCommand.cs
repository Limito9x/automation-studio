using System.Text.Json.Serialization;
using Automation.Identity.Domain.Enums;

namespace Automation.Identity.Features.Users.UpdateUser;

public class UpdateUserCommand
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}



