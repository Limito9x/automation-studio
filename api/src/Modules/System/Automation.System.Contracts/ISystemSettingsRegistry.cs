using FluentResults;

namespace Automation.SystemAbstractions;

public interface ISystemSettingsRegistry
{
    Result<RegisteredSystemSetting> GetSetting(string key);
    IEnumerable<RegisteredSystemSetting> GetAllSettings();
}



