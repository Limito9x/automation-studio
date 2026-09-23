namespace Automation.Tag.Features.Tags.UpdateTag;

public record UpdateTagCommand(Guid Id, string? Name = null, string? Color = null, string? Description = null);

public class UpdateTagValidator : Validator<UpdateTagCommand>
{
    public UpdateTagValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}