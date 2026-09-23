using Automation.Files.Contracts;
using FluentValidation;

namespace Automation.Files.Features.Assets.RequestUpload;

public class RequestUploadValidator : AbstractValidator<RequestUploadCommand>
{
    public RequestUploadValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Items cannot be empty.");
        RuleForEach(x => x.Items).SetValidator(new UploadRequestItemValidator());
    }
}

public class UploadRequestItemValidator : AbstractValidator<UploadRequestItemDto>
{
    public UploadRequestItemValidator()
    {
        RuleFor(x => x.HashSha256)
            .NotEmpty().WithMessage("Hash is required.")
            .Length(64).WithMessage("Hash must be exactly 64 characters long.");

        RuleFor(x => x.Extension)
            .NotEmpty().WithMessage("Extension is required.")
            .Must(x => x.StartsWith(".")).WithMessage("Extension must start with a dot.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Size must be greater than 0.");
            
        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.");
    }
}



