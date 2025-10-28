using System.Text.Json.Serialization;

namespace TagGame.Client.Core.Http;

/// <summary>
/// RFC7807 problem details payload used by the API (application/problem+json).
/// </summary>
public sealed class ApiProblemDetails
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("status")] public int? Status { get; init; }
    [JsonPropertyName("detail")] public string? Detail { get; init; }
    [JsonPropertyName("instance")] public string? Instance { get; init; }

    // Common extensions we rely on
    [JsonPropertyName("traceId")] public string? TraceId { get; init; }

    // Validation errors (ASP.NET HttpValidationProblemDetails)
    [JsonPropertyName("errors")] public Dictionary<string, string[]>? Errors { get; init; }

    // Unknown vendor-specific extensions land here if needed
    [JsonExtensionData] public Dictionary<string, object?> Extensions { get; init; } = [];
}
