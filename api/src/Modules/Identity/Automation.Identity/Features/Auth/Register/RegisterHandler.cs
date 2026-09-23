using Automation.Identity.Domain;
using Automation.Identity.Constants;
using Automation.SystemAbstractions;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Auth.Register;

public class RegisterHandler(
    UserManager<User> userManager,
    IMessageBus bus)
{
    public async Task<Result> HandleAsync(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DisplayName = $"{request.FirstName} {request.LastName}".Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(e.Description)).ToList();
            return Result.Fail(errors);
        }

        var defaultRoleResult = await bus.InvokeAsync<Result<string>>(new GetSystemSettingByKeyQuery(IdentitySettings.DefaultRole), cancellationToken);
        var defaultRole = defaultRoleResult.IsSuccess ? defaultRoleResult.Value : "user";
        await userManager.AddToRoleAsync(user, defaultRole);

        return Result.Ok();
    }
}



