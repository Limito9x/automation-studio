using Automation.SystemAbstractions;
using FluentResults;

namespace Automation.SystemModule.Infrastructure;

public class SystemSettingsRegistry : ISystemSettingsRegistry
{
    private readonly Dictionary<string, RegisteredSystemSetting> _settings = new(StringComparer.OrdinalIgnoreCase);

    public SystemSettingsRegistry(IEnumerable<RegisteredSystemSetting> registeredSettings)
    {
        foreach (var setting in registeredSettings)
        {
            _settings[setting.Key] = setting;
        }
    }

    public Result<RegisteredSystemSetting> GetSetting(string key)
    {
        if (_settings.TryGetValue(key, out var setting))
        {
            return Result.Ok(setting);
        }

        return Result.Fail($"System setting with key '{key}' is not registered.");
    }

    public IEnumerable<RegisteredSystemSetting> GetAllSettings()
    {
        return _settings.Values;
    }
}



