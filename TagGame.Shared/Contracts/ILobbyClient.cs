namespace TagGame.Shared.Contracts;

public interface ILobbyClient
{
    Task LobbyState();

    Task PlayerJoined();

    Task PlayerLeft();

    Task PlayerRoleChanged();

    Task SettingsUpdated();

    Task GameStarted();
}
