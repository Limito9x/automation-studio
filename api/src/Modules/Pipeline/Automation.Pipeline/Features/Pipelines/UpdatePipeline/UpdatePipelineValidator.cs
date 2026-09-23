namespace Automation.Pipeline.Features.Pipelines.UpdatePipeline;

public class UpdatePipelineValidator : Validator<UpdatePipelineRequest>
{
    public UpdatePipelineValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pipeline name is required.")
            .MaximumLength(100).WithMessage("Pipeline name cannot exceed 100 characters.");
    }
}
