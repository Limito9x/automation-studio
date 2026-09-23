using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.RegisterAgent;

public class RegisterAgentEndpoint(IMessageBus bus) : Endpoint<RegisterAgentCommand, RegisterAgentResultDto>
{
    public override void Configure()
    {
        Post("/register");
        Group<AgentsGroup>();
        Permissions(P.Agent.Create);
    }

    public override async Task HandleAsync(RegisterAgentCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<RegisterAgentResultDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

