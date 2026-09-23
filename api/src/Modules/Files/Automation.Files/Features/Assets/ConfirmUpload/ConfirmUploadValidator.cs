using FluentValidation;

namespace Automation.Files.Features.Assets.ConfirmUpload;

public class ConfirmUploadValidator : AbstractValidator<ConfirmUploadCommand>
{
    public ConfirmUploadValidator()
    {
        RuleFor(x => x.AssetIds)
            .NotEmpty().WithMessage("AssetIds cannot be empty.");
        RuleForEach(x => x.AssetIds)
            .NotEmpty().WithMessage("AssetId cannot be empty.");
    }
}



