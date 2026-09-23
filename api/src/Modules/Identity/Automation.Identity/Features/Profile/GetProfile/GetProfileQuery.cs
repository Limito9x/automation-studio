namespace Automation.Identity.Features.Profile.GetProfile;

public record GetProfileQuery(Guid UserId);

public record GetProfileResult(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    string PhoneNumber,
    string? AvatarUrl
);



