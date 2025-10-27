using Microsoft.AspNetCore.Mvc;
using TagGame.Shared.Json;

namespace TagGame.Api.Extensions;

public static class ServiceCollectionJsonOptionsExtensions
{
    /// <summary>
    /// Configure ASP.NET Core JSON serializers (Minimal APIs and MVC) to use shared defaults.
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
