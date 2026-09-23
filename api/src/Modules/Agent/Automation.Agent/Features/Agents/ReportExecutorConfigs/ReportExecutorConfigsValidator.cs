namespace Automation.Agent.Features.Agents.ReportExecutorConfigs;

public class ReportExecutorConfigsValidator : Validator<ReportExecutorConfigsCommand>
{
    public ReportExecutorConfigsValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.Configs).NotNull();
        RuleForEach(x => x.Configs).ChildRules(cfg =>
        {
            cfg.RuleFor(c => c.ExecutorKey).NotEmpty().MaximumLength(50);
            cfg.RuleFor(c => c.ExecutablePath).NotEmpty().MaximumLength(500);
            cfg.RuleFor(c => c.Version).MaximumLength(50);
        });
    }
}
