using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Storage;
using TagGame.Client.Infrastructure.Connectivity;
using TagGame.Client.Infrastructure.Navigation;
using TagGame.Client.Infrastructure.Localization;
using TagGame.Client.Infrastructure.Preferences;
using TagGame.Client.Infrastructure.Storage;

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
        services.AddStorages();
        services.AddLocalization();

        services.AddSingleton<INetworkConnectivity, NetworkConnectivity>();
        services.AddSingleton<IAppPreferences, AppPreferences>();
        services.AddSingleton<INavigationService, ShellNavService>();

        return services;
    }

    private static IServiceCollection AddMauiDefaults(this IServiceCollection services)
    {
        services.AddSingleton(Microsoft.Maui.Storage.Preferences.Default);
        services.AddSingleton(Microsoft.Maui.Storage.SecureStorage.Default);

        return services;
    }

    private static IServiceCollection AddStorages(this IServiceCollection services)
    {
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<IProtectedStorage, ProtectedStorage>();
        services.AddSingleton(typeof(IDataStore<>), typeof(DataStore<>));

        return services;
    }

    private static IServiceCollection AddLocalization(this IServiceCollection services)
    {
        services.AddSingleton<ILocalizationCatalog, ResxCatalog>();
        services.AddSingleton<ILocalizer, Localizer>();
        services.AddSingleton<LocalizationInitializer>();

        return services;
    }
}
