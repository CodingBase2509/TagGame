using Carter;
using Carter.OpenApi;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TagGame.Api.Infrastructure.Health;

namespace TagGame.Api.Endpoints;

public sealed class HealthModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        // Readiness: run checks tagged as ready/readiness (e.g., DB)
        app.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready") || r.Tags.Contains("readiness"),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK, // warn but ready
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = IetfHealthResponseWriter.WriteResponse
        });

        // Liveness: do not run any checks; returns 200 if process is alive
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = IetfHealthResponseWriter.WriteResponse
        });

        app.MapGet("/ping", () => Results.Ok(new { pong = true, at = DateTimeOffset.UtcNow }))
            .WithTags("Health")
            .IncludeInOpenApi();
    }
}
