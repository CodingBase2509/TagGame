using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TagGame.Client.Core.Http;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;

namespace TagGame.Client.Core.Extensions;

/// <summary>
/// ServiceCollection extensions for configuring the typed HTTP API client and handlers.
/// </summary>
public static class ServiceCollectionHttpExtensions
{
    /// <summary>
    /// Registers the typed API client and configures the HTTP pipeline (decompression, handlers, resilience).
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="baseAddressConfiguration">Function that resolves the base address from configuration (e.g., platform specific).</param>
    public static IServiceCollection AddHttpServices(this IServiceCollection services, IConfiguration configuration, Func<IConfiguration, string?> baseAddressConfiguration)
    {
        services.AddTransient<ProblemDetailsHandler>();
        services.AddTransient<AuthorizedHttpHandler>();

        var baseAddress = baseAddressConfiguration(configuration) ?? "http://localhost:5240";

        var httpBuilder = services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate |
                                         DecompressionMethods.Brotli,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            })
            .AddHttpMessageHandler<ProblemDetailsHandler>()
            .AddHttpMessageHandler<AuthorizedHttpHandler>();

        services.ConfigureResilienceHandler(httpBuilder);

        return services;
    }

    /// <summary>
    /// Applies the custom resilience configuration to the given HttpClient builder using registered options/services.
    /// </summary>
    private static void ConfigureResilienceHandler(this IServiceCollection services, IHttpClientBuilder builder)
    {
        var provider = services.BuildServiceProvider();
        var configurator = provider.GetRequiredService<IHttpResilienceConfigurator>();
        var options = provider.GetRequiredService<IOptions<NetworkResilienceOptions>>();

        configurator.Configure(builder, options.Value.Http);
    }
}
