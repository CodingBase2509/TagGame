using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TagGame.Api.Core.Common.Exceptions;

namespace TagGame.Api.Middleware;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, errors, headers) = MapException(exception);
        logger.LogError(exception, "Unhandled exception -> ProblemDetails {Status}", status);

        if (headers is not null)
        {
            foreach (var kv in headers)
            {
                httpContext.Response.Headers[kv.Key] = kv.Value;
            }
        }

        httpContext.Response.StatusCode = status;

        if (errors is not null)
        {
            var validation = new HttpValidationProblemDetails(errors)
            {
                Status = status,
                Title = title
            };

            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = validation,
                Exception = exception,
            });
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title
        };

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static (int status, string title, IDictionary<string, string[]>? errors, Dictionary<string, string>? headers) MapException(Exception ex) =>
        ex switch
        {
            FluentValidation.ValidationException fve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ToDictionary(fve),
                null
            ),
            DomainRuleViolationException dve => (
                StatusCodes.Status422UnprocessableEntity,
                "Domain rule violated",
                dve.Errors as IDictionary<string, string[]> ?? new Dictionary<string, string[]> { [""] = [dve.Message] },
                null
            ),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found", null, null),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", null, null),
            ConflictException or ConcurrencyException => (StatusCodes.Status409Conflict, "Conflict", null, null),
            RateLimitExceededException rle => (
                StatusCodes.Status429TooManyRequests,
                "Too many requests",
                null,
                ToRetryAfterHeaders(rle)
            ),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument", null, null),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found", null, null),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", null, null),
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Not implemented", null, null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", null, null)
        };

    private static Dictionary<string, string[]> ToDictionary(FluentValidation.ValidationException ve) =>
        ve.Errors
            .GroupBy(e => e.PropertyName ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

    private static Dictionary<string, string>? ToRetryAfterHeaders(RateLimitExceededException ex)
    {
        if (ex.RetryAfter is null)
            return null;

        var seconds = Math.Max(0, (int)ex.RetryAfter.Value.TotalSeconds);
        return new Dictionary<string, string>
        {
            ["Retry-After"] = seconds.ToString()
        };
    }
}
