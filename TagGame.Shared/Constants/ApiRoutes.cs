using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Shared.Constants;

public static class ApiRoutes
{
    public static class Initial
    {
        public const string CreateUser = "/users";
    }
    
    public static class GameRoom
    {
        public const string GroupName = "/gameroom";

        public const string CreateRoom = "/";

        public const string JoinRoom = "/{roomId:guid}";

        public const string GetRoom = "/{roomId:guid}";

        public const string UpdateSettings = "/{roomId:guid}/settings";
    }

    public interface GameClient
    {
        
    }

    public interface GameHub
    {
        
    }

    public interface ILobbyClient
    {
        Task ReceiveGameRoomInfo(Domain.Games.GameRoom gameRoom);
        Task ReceivePlayerJoined(Player player);
        Task ReceivePlayerLeft(Guid playerId);
        Task ReceiveGameSettingsUpdated(GameSettings settings);
        Task StartCountdown(int seconds);
    }


    public interface ILobbyHub
    {
        Task UpdateGameSettings(GameSettings settings);
        Task StartGame();
    }

}
