namespace Automation.Content.Features.ContentTypes.CreateContentType;

public class CreateContentTypeValidator : Validator<CreateContentTypeCommand>
{
    public CreateContentTypeValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Icon).MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.DisplayConfig).NotNull();
    }
}

