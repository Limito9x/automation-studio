namespace Automation.Identity.Features.Users.CreateUser;

public record CreateUserCommand(string Username, string Email, string FirstName, string LastName, Guid RoleId);



