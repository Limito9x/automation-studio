namespace Automation.Identity.Features.Profile.UpdateAvatar;

public class UpdateAvatarEndpoint(IMessageBus bus) : Endpoint<UpdateAvatarCommand, string>
{
    public override void Configure()
    {
        Put("/avatar");
        Group<ProfileGroup>();
    }

    public override async Task HandleAsync(UpdateAvatarCommand req, CancellationToken ct)
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



