using FluentResults;

namespace Automation.SharedKernel.Errors;

/// <summary>
/// L?i 401 — dùng ErrorCode d? phân bi?t lo?i l?i:
/// UNAUTHORIZED, TOKEN_EXPIRED, TOKEN_REVOKED, TOKEN_MISSING
/// </summary>
public class UnauthorizedError(string message, string errorCode = "UNAUTHORIZED") : Error(message)
{
    public string ErrorCode { get; } = errorCode;
}



