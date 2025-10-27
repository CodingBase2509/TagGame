namespace TagGame.Api.Extensions;

public static class ServiceCollectionCorsExtensions
{
    public const string DevCorsPolicy = "DevCors";

    public static IServiceCollection AddDevCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevCorsPolicy, policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    // Fallback: allow loopback origins on any port (useful for local dev tools)
                    policy.SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var u) && u.IsLoopback)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}
