using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TagGame.Api.Extensions;

/// <summary>
/// ServiceCollection extensions for host health and readiness checks.
/// </summary>
public static class ServiceCollectionHealthChecksExtensions
{
    private static readonly string[] Tags = ["db", "sql", "readiness", "ready"];

    /// <summary>
    /// Registers health checks (DB readiness, etc.) and configures standard tags.
    /// </summary>
    public static IServiceCollection AddHostHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var hc = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            hc.AddNpgSql(
                connectionString: connectionString!,
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: Tags);
        }

        // add more checks (e.g., cache, external APIs) here as needed

        return services;
    }
}
