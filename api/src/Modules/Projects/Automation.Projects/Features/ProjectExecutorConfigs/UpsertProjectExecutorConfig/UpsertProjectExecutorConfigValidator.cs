namespace Automation.Projects.Features.ProjectExecutorConfigs.UpsertProjectExecutorConfig;

public class UpsertProjectExecutorConfigValidator : AbstractValidator<UpsertProjectExecutorConfigCommand>
{
    public UpsertProjectExecutorConfigValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.ExecutorKey).NotEmpty().MaximumLength(50);
    }
}
