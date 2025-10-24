using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TagGame.Api.Extensions;

public static class HealthChecksExtensions
{
    private static readonly string[] Tags = ["db", "sql", "readiness", "ready"];

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
