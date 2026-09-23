namespace Automation.SharedKernel.Domain.Entities;

public static class IdGenerator
{
    /// <summary>
    /// Generates a time-ordered, sortable UUIDv7 identifier (RFC 9562).
    /// </summary>
    public static Guid NewId() => Guid.CreateVersion7();
}
