namespace Automation.Workspace.Features.ResourceVersions.UpdateResourceMetadata;

public class UpdateResourceMetadataValidator : AbstractValidator<UpdateResourceMetadataCommand>
{
    public UpdateResourceMetadataValidator()
    {
        RuleFor(x => x.ResourceVersionId)
            .NotEmpty()
            .WithMessage("ResourceVersionId is required.");
    }
}
