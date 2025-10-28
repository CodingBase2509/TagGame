using System.Net;

namespace TagGame.Client.Core.Http;

/// <summary>
/// Exception thrown for API responses with application/problem+json or non-success status codes.
/// Consumers can catch this to access typed details.
/// </summary>
public sealed class ApiProblemException(HttpStatusCode statusCode, string message, ApiProblemDetails? problem = null)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public ApiProblemDetails? Problem { get; } = problem;
    public string? TraceId => Problem?.TraceId;
}
