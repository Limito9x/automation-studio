using Automation.Identity.Domain.Enums;

namespace Automation.Identity.Shared.Dtos;

public record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    UserStatus Status,
    string PhoneNumber,
    DateTimeOffset CreatedAt,
    IEnumerable<string> Roles,
    IEnumerable<Guid> RoleIds
);



