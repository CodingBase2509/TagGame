using TagGame.Client.Core.Services.Abstractions;

namespace TagGame.Client.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<INetworkConnectivity, INetworkConnectivity>();
        return services;
    }
}
