namespace Automation.Tag.Features.TagLinks.CreateTagLink;

public class CreateTagLinkValidator : Validator<CreateTagLinkCommand>
{
    public CreateTagLinkValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.TagPath)
            .NotEmpty()
            .Matches(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$")
            .WithMessage("Tag path must only contain alphanumeric characters and underscores separated by dots (e.g. Asset.Character.Hero).");
        RuleFor(x => x.TargetSubPath).MaximumLength(255);
    }
}
