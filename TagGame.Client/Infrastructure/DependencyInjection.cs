using TagGame.Client.Core.Services.Abstractions;
using TagGame.Client.Infrastructure.Connectivity;
using TagGame.Client.Infrastructure.Preferences;

namespace TagGame.Client.Infrastructure;

/// <summary>
/// ServiceCollection extensions for client infrastructure (platform adapters, preferences).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers client infrastructure services (network connectivity, app preferences).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMauiDefaults();

        services.AddSingleton<INetworkConnectivity, NetworkConnectivity>();
        services.AddSingleton<IAppPreferences, AppPreferences>();
        return services;
    }

    private static IServiceCollection AddMauiDefaults(this IServiceCollection services)
    {
        services.AddSingleton(Microsoft.Maui.Storage.Preferences.Default);
        services.AddSingleton(Microsoft.Maui.Storage.SecureStorage.Default);

        return services;
    }
}
