using FluentValidation;

namespace Automation.Pipeline.Features.Pipelines.CreatePipeline;

public class CreatePipelineValidator : AbstractValidator<CreatePipelineCommand>
{
    public CreatePipelineValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
