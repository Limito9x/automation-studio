using FastEndpoints;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Automation.SharedKernel.Errors;

namespace Automation.SharedKernel.Extensions.Results;

public static class ResultExtensions
{
    public static async Task SendResultAsync<T>(
        this BaseEndpoint ep,
        Result<T>? result,
        CancellationToken ct,
        string? message = ""
    )
    {
        if (result is null)
        {
            await SendErrorResponseAsync(ep, [new Error("Internal error: handler returned null result")], ct);
            return;
        }

        if (result.IsSuccess)
        {
            await ep.HttpContext.Response.SendAsync(
                result.Value,
                cancellation: ct
            );
            return;
        }

        await SendErrorResponseAsync(ep, result.Errors, ct);
    }

    public static async Task SendResultAsync(
        this BaseEndpoint ep,
        Result? result,
        CancellationToken ct,
        string? message = ""
    )
    {
        if (result is null)
        {
            await SendErrorResponseAsync(ep, [new Error("Internal error: handler returned null result")], ct);
            return;
        }

        if (result.IsSuccess)
        {
            await ep.HttpContext.Response.SendAsync(
                new { Message = message ?? "Success" },
                cancellation: ct
            );
            return;
        }

        await SendErrorResponseAsync(ep, result.Errors, ct);
    }

    private static async Task SendErrorResponseAsync(
        BaseEndpoint ep,
        IEnumerable<IError> errors,
        CancellationToken ct)
    {
        var error = errors.OfType<IError>().FirstOrDefault();

        var (statusCode, errorCode) = error switch
        {
            NotFoundError => (404, "NOT_FOUND"),
            ConflictError => (409, "CONFLICT"),
            ValidationError => (400, "VALIDATION"),
            UnauthorizedError e => (401, e.ErrorCode),
            ForbiddenError => (403, "FORBIDDEN"),
            _ => (500, "SERVER_ERROR"),
        };

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = error?.Message ?? "An error occurred",
            Detail = string.Join("; ", errors.Select(e => e.Message)),
            Extensions = { ["errorCode"] = errorCode }
        };

        ep.HttpContext.Response.StatusCode = statusCode;
        await HttpResponseJsonExtensions.WriteAsJsonAsync(ep.HttpContext.Response, problemDetails, cancellationToken: ct);
    }
}



