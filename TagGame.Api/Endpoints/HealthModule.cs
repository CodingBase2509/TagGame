using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace TagGame.Api.Endpoints;

public sealed class HealthModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        // Readiness: evaluates registered health checks (e.g., DB)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Liveness: does not run checks; returns 200 if process is alive
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.MapGet("/ping", () => Results.Ok(new { pong = true, at = DateTimeOffset.UtcNow }));
    }
}

