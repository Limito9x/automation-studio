using FluentValidation;

namespace Automation.Pipeline.Features.Nodes.CreateCustomNode;

public class CreateCustomNodeValidator : AbstractValidator<CreateCustomNodeCommand>
{
    public CreateCustomNodeValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
