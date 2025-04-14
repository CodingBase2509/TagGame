namespace TagGame.Client.Ui;

public interface INavigation
{
    Task GoToLobby(NavigationMode mode, Dictionary<string, object>? navItems = null);

    Task GoToStart(NavigationMode mode, Dictionary<string, object>? navItems = null);

    Task GoToInit(NavigationMode mode, Dictionary<string, object>? navItems = null);
}