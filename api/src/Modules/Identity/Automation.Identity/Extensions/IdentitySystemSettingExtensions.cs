using Automation.SystemAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Identity.Extensions;

public static class IdentitySystemSettingExtensions
{
    public static IServiceCollection AddIdentitySystemSettings(this IServiceCollection services)
    {
        services.AddSystemSetting(Automation.Identity.Constants.IdentitySettings.DefaultRole, Automation.Identity.Constants.IdentityRoles.User, "string", "Vai trò mặc định cho người dùng mới đăng ký");

        return services;
    }
}



