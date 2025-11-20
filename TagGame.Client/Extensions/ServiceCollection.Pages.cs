using TagGame.Client.Core.Services;
using TagGame.Client.Ui.Views.Game;
using TagGame.Client.Ui.Views.Lobby;
using TagGame.Client.Ui.Views.Settings;
using TagGame.Client.Ui.Views.Start;

namespace TagGame.Client.Extensions;

public static class ServiceCollectionPages
{
    public static IServiceCollection AddPages(this IServiceCollection services)
    {
        services.AddTransient<Start>();
        services.AddTransient<UserInitModal>();

        services.AddTransient<LobbyPage>();
        services.AddTransient<LobbySettingsPage>();
        services.AddTransient<QrCodeModal>();
        services.AddTransient<GeofenceSelectionPage>();

        services.AddTransient<SettingsPage>();
        services.AddTransient<ProfilePage>();

        services.AddTransient<GamePage>();
        services.AddTransient<GameChatPage>();
        services.AddTransient<GamePlayerListPage>();

        services.AddSingleton<IUiDispatcher, UiUtils>();

        return services;
    }
}
