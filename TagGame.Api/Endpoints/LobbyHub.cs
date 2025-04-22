using Microsoft.AspNetCore.SignalR;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Endpoints;

public class LobbyHub : Hub<ApiRoutes.ILobbyClient>, ApiRoutes.ILobbyHub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        
        // set connectionId on Player
    }

    public Task UpdateGameSettings(GameSettings settings)
    {
        throw new NotImplementedException();
    }

    public Task StartGame()
    {
        throw new NotImplementedException();
    }
}
