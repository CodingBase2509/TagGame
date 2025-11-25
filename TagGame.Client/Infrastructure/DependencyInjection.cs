using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Core.Storage;
using TagGame.Client.Infrastructure.Connectivity;
using TagGame.Client.Infrastructure.Navigation;
using TagGame.Client.Infrastructure.Localization;
using TagGame.Client.Infrastructure.Preferences;
using TagGame.Client.Infrastructure.QrCodes;
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
        services.AddMauiDefaults()
            .AddStorages()
            .AddLocalization()
            .AddToasts();

        services.AddSingleton<INetworkConnectivity, NetworkConnectivity>();
        services.AddSingleton<IAppPreferences, AppPreferences>();
        services.AddSingleton<INavigationService, ShellNavService>();
        services.AddTransient<IQrCodeService, QrCodeService>();

        return services;
    }

    private static IServiceCollection AddMauiDefaults(this IServiceCollection services)
    {
        services.AddSingleton<ISecureStorage>(_ => SecureStorage.Default);
        services.AddSingleton<IPreferences>(_ => Microsoft.Maui.Storage.Preferences.Default);

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

    private static IServiceCollection AddToasts(this IServiceCollection services)
    {
        services.AddSingleton<ToastPublisher>();
        services.AddSingleton<IToastSender>(sp => sp.GetRequiredService<ToastPublisher>());
        services.AddSingleton<IToastPublisher>(sp => sp.GetRequiredService<ToastPublisher>());
        services.AddSingleton<ToastPresenter>();

        return services;
    }
}
