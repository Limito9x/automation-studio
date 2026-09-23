namespace Automation.SharedKernel.Abstractions.Cursor;

public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor, bool HasMore);


