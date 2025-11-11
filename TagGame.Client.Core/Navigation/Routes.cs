namespace TagGame.Client.Core.Navigation;

public class Routes
{
    // Pages / Routen
    public const string Start = "start";
    public const string UserInit = "init";

    public const string Lobby = "lobby";
    public const string LobbySettings = "lobbySettings";
    public const string QrModal = "qrModal";
    public const string GeofenceSelect = "gefoeceSelect";

    public const string Game = "game";
    public const string GameChat = "gameChat";
    public const string GamePlayers = "gamePlayers";

    public const string Settings = "settings";
    public const string Profile = "profile";

    // Query-/Route-Parameter
    public static class Params
    {
        public const string RoomId = "roomId";
        public const string GameId = "gameId";
        public const string UserId = "userId";
    }
}
