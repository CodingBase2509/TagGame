namespace TagGame.Api.Endpoints;

public abstract class EndpointBase
{
    // 2xx
    protected static IResult Ok<T>(T value) => Results.Ok(value);
    protected static IResult NoContent() => Results.NoContent();
    protected static IResult Created(string location, object? value = null) => Results.Created(location, value);

    // 3xx
    protected static IResult NotModified() =>
        Results.StatusCode(StatusCodes.Status304NotModified);

    // 4xx (ProblemDetails)
    protected static IResult BadRequest(string detail, string? code = null, string? title = null) =>
        Problem(StatusCodes.Status400BadRequest, title ?? "Bad request", detail, code);

    protected static IResult Unauthorized(string detail, string? code = null) =>
        Problem(StatusCodes.Status401Unauthorized, "Unauthorized", detail, code);

    protected static IResult Forbidden(string detail, string? code = null) =>
        Problem(StatusCodes.Status403Forbidden, "Forbidden", detail, code);

    protected static IResult NotFound(string detail, string? code = null) =>
        Problem(StatusCodes.Status404NotFound, "Not found", detail, code);

    protected static IResult Conflict(string detail, string? code = null) =>
        Problem(StatusCodes.Status409Conflict, "Conflict", detail, code);

    protected static IResult PreconditionFailed(string detail, string? code = null) =>
        Problem(StatusCodes.Status412PreconditionFailed, "Precondition failed", detail, code);

    protected static IResult UnprocessableEntity(string detail, string? code = null) =>
        Problem(StatusCodes.Status422UnprocessableEntity, "Unprocessable entity", detail, code);

    protected static IResult PreconditionRequired(string details, string? code = null) =>
        Problem(StatusCodes.Status428PreconditionRequired, "Precondition required", details, code);

    protected static IResult TooManyRequests(string detail, string? code = null) =>
        Problem(StatusCodes.Status429TooManyRequests, "Too many requests", detail, code);

    // 5xx (Optional)
    protected static IResult InternalError(string detail, string? code = null) =>
        Problem(StatusCodes.Status500InternalServerError, "Internal server error", detail, code);

    // Centralized ProblemDetails-Creation with additional "code"
    protected static IResult Problem(
        int status,
        string title,
        string detail,
        string? code = null,
        string? type = null,
        string? instance = null,
        IDictionary<string, object?>? extensions = null)
    {
        var ext = extensions is not null
            ? new Dictionary<string, object?>(extensions)
            : [];

        if (!string.IsNullOrWhiteSpace(code))
            ext["code"] = code;

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: detail,
            type: type,
            instance: instance,
            extensions: ext);
    }
}
