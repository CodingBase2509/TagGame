using Microsoft.AspNetCore.Mvc;
using TagGame.Shared.Json;

namespace TagGame.Api.Extensions;

/// <summary>
/// ServiceCollection extensions to align ASP.NET Core JSON options with shared defaults.
/// </summary>
public static class ServiceCollectionJsonOptionsExtensions
{
    /// <summary>
    /// Configure ASP.NET Core JSON serializers (Minimal APIs and MVC) to use shared defaults.
    /// </summary>
    /// <summary>
    /// Configures Minimal API (HttpJsonOptions) and MVC (JsonOptions) to use shared JSON defaults.
    /// </summary>
    public static IServiceCollection AddSharedJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(static o => JsonDefaults.Apply(o.SerializerOptions));

        // Ensure MVC/ProblemDetails serialization uses the same options.
        services.Configure<JsonOptions>(static o =>
        {
            JsonDefaults.Apply(o.JsonSerializerOptions);
        });

        return services;
    }
}
