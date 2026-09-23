using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Automation.Notifications.Infrastructure.Persistence;

using Automation.Notifications.Domain.Interfaces;
using Automation.Notifications.Infrastructure.Services;

namespace Automation.Notifications;

public sealed class NotificationsModule : IModule
{
    public string Name => "Notifications";
    public string SchemaName => "notifications";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<NotificationsDbContext>(config, SchemaName);
        services.AddTransient<IEmailSender, MailKitEmailSender>();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        options.PublishMessage<Contracts.Messages.SendEmailCommand>()
               .ToLocalQueue("email-queue")
               .UseDurableInbox();
    }

    public List<Type> Endpoints => [..DiscoveredTypes.All];
}



