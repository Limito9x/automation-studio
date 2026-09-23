namespace Automation.Tag.Features.Tags.CreateTag;

public record CreateTagCommand(
    Guid ProjectId,
    string Path,
    string? Color = null,
    string? Description = null
);

public class CreateTagValidator : Validator<CreateTagCommand>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Path)
            .NotEmpty()
            .Matches(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$")
            .WithMessage("Tag path must only contain alphanumeric characters and underscores separated by dots (e.g. Asset.Character.Hero).");
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}