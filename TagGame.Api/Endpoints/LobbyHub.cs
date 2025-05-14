using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Services;
using TagGame.Api.Validation.GameRoom;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Endpoints;

[Authorize]
public class LobbyHub(
    GameRoomService gameRooms,
    GameRoomSettingsValidator validator,
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

        var playerLeftInfo = await players.GetPlayerLeftGame(player.Id);
        // inform others, player left
        if (playerLeftInfo is null)
        {
            playerLeftInfo = new PlayerLeftGameInfo()
            {
                Id = Guid.Empty,
                DisconnectType = PlayerDisconnectType.LeftWithReconnect,
                Player = player,
            };
        }
        else
        {
            // update players on room
            var success = await players.RemovePlayerFromRoomAsync(player.Id, gameRoom.Id);
            if (success)
                success &= await CheckRoomAdminAndUpdate(player, gameRoom);

            if (success)
            {
                await players.DeletePlayerLeftGameAsync(playerLeftInfo.Id);
                await players.DeletePlayerAsync(player.Id);
            }
        }
        
        await Clients.OthersInGroup(gameRoom.Id.ToString()).ReceivePlayerLeft(playerLeftInfo);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameRoom.Id.ToString());
        
        // check if any player is connected to room
        gameRoom = await gameRooms.GetRoomAsync(gameRoom.Id);
        if (gameRoom is null || gameRoom.Players.Count > 0)
            return;
        
        // no player & game still in lobby: close game room
        var deleteSuccess = await gameRooms.DeleteRoomAsync(gameRoom.Id);
    }

    public async Task ReceiveDisconnectInfo()
    {
        var leftInfo = await players.CreatePlayerLeftGameAsync(Context.ConnectionId);
        if (leftInfo is null)
            return;
        
        Console.WriteLine(leftInfo.Player.UserName);
    }

    public async Task UpdateGameSettings(GameSettings settings)
    {
        var validationResult = await validator.ValidateAsync(settings);
        if (!validationResult.IsValid)
            return;
        
        await gameRooms.UpdateSettingsAsync(settings.RoomId, settings);
        
        await Clients.OthersInGroup(settings.RoomId.ToString())
            .ReceiveGameSettingsUpdated(settings);
    }

    public Task StartGame()
    {
        throw new NotImplementedException();
    }

    private async Task<bool> CheckRoomAdminAndUpdate(Player player, GameRoom room)
    {
        if (!Equals(room.OwnerUserId, player.UserId))
            return true;
        
        var nextPlayer = room.Players
            .FirstOrDefault(p => !Equals(player.Id, p.Id));
        
        if (nextPlayer is null)
            return true;
        
        var success = await gameRooms.UpdateRoomOwnerAsync(room.Id, nextPlayer);
        if (success)
            await Clients.Group(room.Id.ToString()).ReceiveNewRoomOwner(nextPlayer.UserId);
        
        return success;
    }
    
    private Guid GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? throw new HubException("Unauthorized");
        return Guid.Parse(claim.Value);
    }
}
