namespace Automation.SystemModule.Features.SystemSettings.UpdateSystemSetting;

public class UpdateSystemSettingValidator : Validator<UpdateSystemSettingCommand>
{
    public UpdateSystemSettingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}



