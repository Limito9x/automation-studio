using System.Text.Json;
using Automation.SharedKernel.Abstractions.Caching;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Automation.SharedKernel.Infrastructure.Caching;

public class CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger) : ICacheService
{
    private IDatabase Db => redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var cachedValue = await Db.StringGetAsync(key);
            if (!cachedValue.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue.ToString());
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis connection failed while trying to get cache key {Key}. Skipping cache.", key);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting cache key {Key}.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken ct = default
    )
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(value);
            var ttl = expiration ?? TimeSpan.FromMinutes(15);
            await Db.StringSetAsync(key, jsonString, ttl);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis connection failed while trying to set cache key {Key}. Skipping cache.", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while setting cache key {Key}.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await Db.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis connection failed while trying to remove cache key {Key}. Skipping cache.", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing cache key {Key}.", key);
        }
    }

    public async Task RemoveAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        try
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            if (redisKeys.Length > 0)
            {
                await Db.KeyDeleteAsync(redisKeys);
            }
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis connection failed while trying to remove multiple cache keys. Skipping cache.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing multiple cache keys.");
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            foreach (var endpoint in redis.GetEndPoints())
            {
                var server = redis.GetServer(endpoint);
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                if (keys.Length > 0)
                {
                    await Db.KeyDeleteAsync(keys);
                }
            }
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis connection failed while trying to remove cache keys by prefix {Prefix}. Skipping cache.", prefix);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing cache keys by prefix {Prefix}.", prefix);
        }
    }
}



