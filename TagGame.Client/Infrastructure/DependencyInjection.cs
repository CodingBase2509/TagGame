using TagGame.Client.Core.Services.Abstractions;
using TagGame.Client.Infrastructure.Connectivity;

namespace TagGame.Client.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<INetworkConnectivity, NetworkConnectivity>();
        return services;
    }
}
