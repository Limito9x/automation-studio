using Automation.SharedKernel.Abstractions.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace Automation.SharedKernel.Infrastructure.Caching;

public static class FusionCacheMiddleware
{
    public static async Task<TResponse?> BeforeAsync<TQuery, TResponse>(
        TQuery query,
        IFusionCache cache)
        where TQuery : ICachedQuery<TResponse>
    {
        // Try to get data from cache. 
        // If data exists, Wolverine will short-circuit the execution and return it immediately.
        var cachedData = await cache.GetOrDefaultAsync<TResponse>(query.CacheKey);
        return cachedData;
    }

    public static async Task AfterAsync<TQuery, TResponse>(
        TQuery query,
        TResponse response,
        IFusionCache cache)
        where TQuery : ICachedQuery<TResponse>
    {
        // Save the result from the handler into the cache
        var duration = query.Expiration ?? TimeSpan.FromMinutes(10);
        await cache.SetAsync(query.CacheKey, response, duration);
    }
}



