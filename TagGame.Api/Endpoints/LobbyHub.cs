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
    PlayerService players) 
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
        
        // send room info to new player
        await Clients.Caller.ReceiveGameRoomInfo(gameRoom);
        
        // inform other players the new player joined
        await Clients.GroupExcept(gameRoom.Id.ToString(), [Context.ConnectionId])
            .ReceivePlayerJoined(player);
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
            // TODO: impl updating room owner if owner leave
            var success = await players.RemovePlayerFromRoomAsync(player.Id, gameRoom.Id);
            if (success)
                success &= await players.DeletePlayerAsync(player.Id);
            
            playerLeftInfo.DisconnectType = PlayerDisconnectType.LeftGame;
        }
        
        await Clients.OthersInGroup(gameRoom.Id.ToString())
            .ReceivePlayerLeft(playerLeftInfo);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameRoom.Id.ToString());
        
        // check if any player is connected to room
        gameRoom = await gameRooms.GetRoomAsync(gameRoom.Id);
        if (gameRoom is null || gameRoom.Players.Count > 0)
            return;
        
        // no player & game still in lobby: close game room
        var deleteSuccess = await gameRooms.DeleteRoomAsync(gameRoom.Id);
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
