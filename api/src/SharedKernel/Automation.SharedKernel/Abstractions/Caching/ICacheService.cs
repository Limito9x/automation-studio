namespace Automation.SharedKernel.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken ct = default
    );
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveAsync(IEnumerable<string> keys, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}



