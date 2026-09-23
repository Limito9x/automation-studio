namespace Automation.SharedKernel.Abstractions.Caching;

public interface ICachedQuery<TResponse>
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}



