using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Automation.SharedKernel.Infrastructure.Caching;
using Automation.SharedKernel.Abstractions.Caching;
using StackExchange.Redis;

namespace Automation.SharedKernel.Extensions.Caching;

public static class CachingServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisConnectionString));
            
        services.AddSingleton<ICacheService, CacheService>();
        
        return services;
    }
}



