using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Client.Clients;

public class LobbyClient : ApiRoutes.ILobbyClient
{
    public Task ReceiveGameRoomInfo(GameRoom gameRoom)
    {
        throw new NotImplementedException();
    }

    public Task ReceivePlayerJoined(Player player)
    {
        throw new NotImplementedException();
    }

    public Task ReceivePlayerLeft(Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task ReceiveGameSettingsUpdated(GameSettings settings)
    {
        throw new NotImplementedException();
    }

    public Task StartCountdown(int seconds)
    {
        throw new NotImplementedException();
    }
}