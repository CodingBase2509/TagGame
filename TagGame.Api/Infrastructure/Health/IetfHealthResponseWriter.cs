using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TagGame.Api.Infrastructure.Health;

internal static class IetfHealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static Task WriteResponse(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/health+json; charset=utf-8";

        var status = report.Status switch
        {
            HealthStatus.Healthy => "pass",
            HealthStatus.Degraded => "warn",
            HealthStatus.Unhealthy => "fail",
            _ => "fail"
        };

        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

        var checks = report.Entries
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => new
                {
                    status = e.Value.Status switch
                    {
                        HealthStatus.Healthy => "pass",
                        HealthStatus.Degraded => "warn",
                        HealthStatus.Unhealthy => "fail",
                        _ => "fail"
                    },
                    time = DateTimeOffset.UtcNow,
                    duration = e.Value.Duration,
                    description = string.IsNullOrWhiteSpace(e.Value.Description) ? null : e.Value.Description,
                    error = e.Value.Exception?.Message
                }).ToArray());

        var payload = new
        {
            status,
            serviceId = "taggame-api",
            version,
            time = DateTimeOffset.UtcNow,
            checks = checks.Count == 0 ? null : checks
        };

        return httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
