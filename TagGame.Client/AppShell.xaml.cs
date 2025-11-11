using TagGame.Client.Core.Navigation;
using TagGame.Client.Ui.Views;
using TagGame.Client.Ui.Views.Game;
using TagGame.Client.Ui.Views.Lobby;
using TagGame.Client.Ui.Views.Settings;

namespace TagGame.Client;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(Routes.UserInit, typeof(UserInitModal));

        Routing.RegisterRoute(Routes.Lobby, typeof(LobbyPage));
        Routing.RegisterRoute(Routes.LobbySettings, typeof(LobbySettingsPage));
        Routing.RegisterRoute(Routes.GeofenceSelect, typeof(GeofenceSelectionPage));
        Routing.RegisterRoute(Routes.QrModal, typeof(QrCodeModal));

        Routing.RegisterRoute(Routes.Game, typeof(GamePage));
        Routing.RegisterRoute(Routes.GameChat, typeof(GameChatPage));
        Routing.RegisterRoute(Routes.GamePlayers, typeof(GamePlayerListPage));

        Routing.RegisterRoute(Routes.Settings, typeof(SettingsPage));
        Routing.RegisterRoute(Routes.Profile, typeof(ProfilePage));
    }
}
