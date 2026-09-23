namespace Automation.Agent.Features.Agents.RegisterAgent;

public class RegisterAgentValidator : Validator<RegisterAgentCommand>
{
    public RegisterAgentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MachineKey).NotEmpty().MaximumLength(255);
    }
}

