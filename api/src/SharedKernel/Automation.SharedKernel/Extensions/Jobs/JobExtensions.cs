using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TickerQ.Caching.StackExchangeRedis.DependencyInjection;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

namespace Automation.SharedKernel.Extensions.Jobs;

public static class JobExtensions
{
    public static IServiceCollection AddJobServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnStr = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var multiplexer = ConnectionMultiplexer.Connect(redisConnStr);

        services.AddTickerQ(options =>
        {
            options.AddStackExchangeRedis(redisOpt =>
            {
                redisOpt.ConnectionMultiplexer = multiplexer;
            });

            options.AddDashboard(dashOpt =>
            {
                var username = configuration["TickerQ:BasicAuth:Username"];
                var password = configuration["TickerQ:BasicAuth:Password"];

                dashOpt.SetBasePath("/tickerq-dashboard");
                
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    dashOpt.WithBasicAuth(username, password);
                }
            });
        });

        return services;
    }

    public static WebApplication UseJobMiddleware(this WebApplication app)
    {
        app.UseTickerQ();
        return app;
    }
}


