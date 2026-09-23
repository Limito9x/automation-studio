namespace Automation.Identity.Features.Profile.GetProfile;

public class GetProfileEndpoint(IMessageBus bus) : EndpointWithoutRequest<GetProfileResult>
{
    public override void Configure()
    {
        Get("/");
        Group<ProfileGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Extract user id from JWT claims
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        var result = await bus.InvokeAsync<Result<GetProfileResult>>(new GetProfileQuery(userId), ct);
        await this.SendResultAsync(result, ct);
    }
}



