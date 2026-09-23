namespace Automation.Identity.Features.Profile.UpdateProfile;

public class UpdateProfileEndpoint(IMessageBus bus) : Endpoint<UpdateProfileCommand, string>
{
    public override void Configure()
    {
        Put("/");
        Group<ProfileGroup>();
    }

    public override async Task HandleAsync(UpdateProfileCommand req, CancellationToken ct)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        req.UserId = userId;
        var result = await bus.InvokeAsync<Result<string>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



