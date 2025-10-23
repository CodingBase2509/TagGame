using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TagGame.Api.Extensions;

public static class HealthChecksExtensions
{
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
                tags: new[] { "db", "sql", "readiness" });
        }

        // add more checks (e.g., cache, external APIs) here as needed

        return services;
    }
}

