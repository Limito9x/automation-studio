using Microsoft.Extensions.Configuration;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Automation.SharedKernel.Extensions.RabbitMq;

public static class WolverineRabbitMqExtensions
{
    public static WolverineOptions UseSharedRabbitMq(this WolverineOptions options, IConfiguration configuration)
    {
        var enabledStr = configuration["RabbitMQ:Enabled"];
        if (bool.TryParse(enabledStr, out var isEnabled) && !isEnabled)
        {
            return options;
        }

        var host = configuration["RabbitMQ:Host"] ?? "localhost";
        var portStr = configuration["RabbitMQ:Port"];
        var port = int.TryParse(portStr, out var p) ? p : 5672;
        var user = configuration["RabbitMQ:Username"] ?? "guest";
        var pass = configuration["RabbitMQ:Password"] ?? "guest";
        var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

        options.UseRabbitMq(transport =>
        {
            transport.HostName = host;
            transport.Port = port;
            transport.UserName = user;
            transport.Password = pass;
            transport.VirtualHost = virtualHost;
            transport.RequestedConnectionTimeout = TimeSpan.FromSeconds(3);
        })
        .AutoProvision();

        return options;
    }
}
