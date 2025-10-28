using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;
using TagGame.Client.Core.Services.Implementations;

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
