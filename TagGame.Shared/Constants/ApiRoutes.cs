namespace TagGame.Shared.Constants;

public static class ApiRoutes
{
    public static class Initial
    {
        public const string CreateUser = "/users/{id:guid}";
    }
    
    public static class GameRoom
    {
        public const string GroupName = "gameroom";

        public const string CreateRoom = "/";

        public const string JoinRoom = "/{roomId:guid}";

        public const string GetRoom = "/{roomId:guid}";

        public const string UpdateSettings = "/{roomId:guid}/settings";
    }

    public static class GameHub
    {
        
    }

    public static class LobbyHub
    {

    }
}
