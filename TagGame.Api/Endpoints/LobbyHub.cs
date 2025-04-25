using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Endpoints;

[Authorize]
public class LobbyHub(
    GameRoomService gameRooms,
    PlayerService players,
    UserService users) 
    : Hub<ApiRoutes.ILobbyClient>, ApiRoutes.ILobbyHub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        
        var userId = GetUserId();
        var player = await players.GetPlayerByUserId(userId);
        if (player is null)
        {
            Context.Abort();
            return;
        }
        
        var gameRoom = await gameRooms.GetRoomFromPlayerAsync(player.Id);
        if (gameRoom is null)
        {
            Context.Abort();
            return;
        }
        
        // set connectionId on Player
        player.ConnectionId = Context.ConnectionId;
        var success = await players.UpdatePlayerAsync(player);
        if (!success)
        {
            Context.Abort();
            await players.RemovePlayerFromRoomAsync(player.Id, gameRoom.Id);
        }
        
        // add player / connection to webSocket group
        await Groups.AddToGroupAsync(Context.ConnectionId, gameRoom.Id.ToString());
        
        // inform other players the new player joined
        await Clients.Group(gameRoom.Id.ToString()).ReceivePlayerJoined(player);
        
        // send room info to new player
        await Clients.Caller.ReceiveGameRoomInfo(gameRoom);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        
        var userId = GetUserId();
        var player = await players.GetPlayerByUserId(userId);
        if (player is null)
        {
            return;
        }
        
        var gameRoom = await gameRooms.GetRoomFromPlayerAsync(player.Id);
        if (gameRoom is null)
        {
            return;
        }
        
        // remove connectionId from player
        player.ConnectionId = null;

        var playerLeftInfo = new PlayerLeftGameInfo()
        {
            Player = player,
        };
        
        // inform others, player left
        if (exception is not null)
        {
            playerLeftInfo.DisconnectType = PlayerDisconnectType.LeftWithReconnect;
        }
        else
        {
            // update players on room
            await players.RemovePlayerFromRoomAsync(player.Id, gameRoom.Id);
            playerLeftInfo.DisconnectType = PlayerDisconnectType.LeftGame;
        }
        
        // check if any player is connected to room
        gameRoom = await gameRooms.GetRoomFromPlayerAsync(player.Id);
        if (gameRoom is null || gameRoom.Players.Count > 0)
            return;
        
        // no player & game still in lobby: close game room
        await gameRooms.DeleteRoomAsync(gameRoom.Id);
    }

    public Task UpdateGameSettings(GameSettings settings)
    {
        throw new NotImplementedException();
    }

    public Task StartGame()
    {
        throw new NotImplementedException();
    }
    
    private Guid GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? throw new HubException("Unauthorized");
        return Guid.Parse(claim.Value);
    }
}
