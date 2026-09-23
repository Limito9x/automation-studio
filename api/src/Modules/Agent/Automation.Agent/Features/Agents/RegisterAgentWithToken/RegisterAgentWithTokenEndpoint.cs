using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.RegisterAgentWithToken;

public class RegisterAgentWithTokenEndpoint(IMessageBus bus) : Endpoint<RegisterAgentWithTokenCommand, RegisterAgentResultDto>
{
    public override void Configure()
    {
        Post("/register-token");
        Group<AgentsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterAgentWithTokenCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<RegisterAgentResultDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
