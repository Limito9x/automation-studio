using System.Text.Json.Serialization;

namespace Automation.Identity.Features.Users.AssignUserRoles;

public class AssignUserRolesCommand
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public List<string> Roles { get; set; } = [];
}



