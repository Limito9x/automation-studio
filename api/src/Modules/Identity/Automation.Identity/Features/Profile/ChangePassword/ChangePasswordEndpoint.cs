using System.Security.Claims;

namespace Automation.Identity.Features.Profile.ChangePassword;

public class ChangePasswordEndpoint(IMessageBus bus) : Endpoint<ChangePasswordCommand, string>
{
    public override void Configure()
    {
        Put("change-password");
        Group<ProfileGroup>();
    }

    public override async Task HandleAsync(ChangePasswordCommand req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdString, out var userId))
        {
            req.UserId = userId;
        }

        var result = await bus.InvokeAsync<FluentResults.Result<string>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



