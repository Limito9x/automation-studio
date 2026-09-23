namespace Automation.SharedKernel.Abstractions.Caching;

public interface IInvalidateCacheCommand
{
    IEnumerable<string> CacheKeysToInvalidate { get; }
}



