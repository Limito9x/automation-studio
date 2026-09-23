namespace Automation.Notifications.Features.Notifications.Archive;

public class ArchiveValidator : Validator<ArchiveCommand>
{
    public ArchiveValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}



