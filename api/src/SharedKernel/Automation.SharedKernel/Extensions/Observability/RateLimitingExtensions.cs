using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.SharedKernel.Extensions.Observability;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.AddSlidingWindowLimiter("Global", limiterOptions =>
            {
                var rateLimitingConfig = configuration.GetSection("RateLimiting:Global");
                
                limiterOptions.PermitLimit = rateLimitingConfig.GetValue("PermitLimit", 100);
                limiterOptions.SegmentsPerWindow = rateLimitingConfig.GetValue("SegmentsPerWindow", 12);
                limiterOptions.QueueLimit = rateLimitingConfig.GetValue("QueueLimit", 2);
                limiterOptions.Window = TimeSpan.FromSeconds(rateLimitingConfig.GetValue("WindowInSeconds", 60));
            });
        });

        return services;
    }
}



