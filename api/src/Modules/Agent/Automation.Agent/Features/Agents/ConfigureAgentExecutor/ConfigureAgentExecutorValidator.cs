namespace Automation.Agent.Features.Agents.ConfigureAgentExecutor;

public class ConfigureAgentExecutorValidator : Validator<ConfigureAgentExecutorCommand>
{
    public ConfigureAgentExecutorValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.ExecutorKey).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ExecutablePath).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Version).MaximumLength(50);
    }
}
