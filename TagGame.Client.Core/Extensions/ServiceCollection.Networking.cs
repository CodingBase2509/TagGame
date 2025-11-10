using Microsoft.Extensions.Configuration;
using TagGame.Client.Core.Http.Configuration;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Extensions;

/// <summary>
/// ServiceCollection extensions for networking resilience (HTTP + SignalR) in the client.
/// </summary>
public static class ServiceCollectionNetworkingExtensions
{
    public static IServiceCollection AddNetworkingResilience(this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration is not null)
        {
            services.AddOptions<NetworkResilienceOptions>()
                .Bind(configuration.GetSection("Networking"))
                .ValidateOnStart();
        }
        else
        {
            services.AddOptions<NetworkResilienceOptions>();
        }

        services.AddSingleton<IHttpResilienceConfigurator, HttpResilienceConfigurator>();
        services.AddSingleton<IHubRetryPolicyFactory, HubRetryPolicyFactory>();

        return services;
    }
}
