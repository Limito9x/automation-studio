namespace Automation.Agent.Features.Agents.RegisterAgentWithToken;

public class RegisterAgentWithTokenValidator : Validator<RegisterAgentWithTokenCommand>
{
    public RegisterAgentWithTokenValidator()
    {
        RuleFor(x => x.SetupToken)
            .NotEmpty().WithMessage("Setup token is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.MachineKey)
            .NotEmpty().WithMessage("Machine key is required");
    }
}
