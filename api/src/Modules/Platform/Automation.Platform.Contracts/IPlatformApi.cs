using FluentResults;

namespace Automation.Platform.Contracts;

public interface IPlatformApi
{
    Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(
        Guid platformId,
        CancellationToken ct = default
    );
    Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(
        IEnumerable<Guid> platformIds,
        CancellationToken ct = default
    );

    Task<Result<Guid?>> GetExtensionIdAsync(
        Guid platformId,
        string extension,
        CancellationToken ct = default
    );

    Task<Result<Dictionary<string, Guid>>> GetExtensionMapAsync(
        IEnumerable<Guid>? platformIds,
        CancellationToken ct = default
    );
}
