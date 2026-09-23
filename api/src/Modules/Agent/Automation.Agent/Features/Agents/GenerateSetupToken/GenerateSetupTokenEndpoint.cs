namespace Automation.Agent.Features.Agents.GenerateSetupToken;

public class GenerateSetupTokenEndpoint(IMessageBus bus) : EndpointWithoutRequest<SetupTokenDto>
{
    public override void Configure()
    {
        Post("/generate-token");
        Group<AgentsGroup>();
        Permissions(P.Agent.Create);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<SetupTokenDto>>(new GenerateSetupTokenCommand(), ct);
        await this.SendResultAsync(result, ct);
    }
}
