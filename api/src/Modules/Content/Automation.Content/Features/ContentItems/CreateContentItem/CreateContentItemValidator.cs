namespace Automation.Content.Features.ContentItems.CreateContentItem;

public class CreateContentItemValidator : Validator<CreateContentItemCommand>
{
    public CreateContentItemValidator()
    {
        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Values).NotNull();
    }
}

