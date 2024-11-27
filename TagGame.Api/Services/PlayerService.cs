using System.Drawing;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Services;

public class PlayerService(IDataSet dataSet)
{
    public async Task<Player?> CreatePlayerAsync(Guid userId)
    {
        var user = await dataSet.Set<User>().FindAsync(userId);
        if (user is null)
            return null;
        
        var player = new Player()
        {
            Id = user.Id,
            AvatarColor = user.DefaultAvatarColor,
            UserName = user.DefaultName,
        };
        
        var entity = await dataSet.Set<Player>().AddAsync(player);
        if (entity.State != EntityState.Added)
            return null;
        
        var changedEntities = await dataSet.SaveChangesAsync();
        return changedEntities == 0 ? null : player;
    }

    public async Task<bool> DeletePlayerAsync(Guid playerId)
    {
        if (Equals(playerId, Guid.Empty))
            return false;
        
        var player = await GetPlayerById(playerId);
        if (player is null)
            return false;
        
        var entity = dataSet.Set<Player>()
            .Remove(player);
        if (entity.State != EntityState.Deleted)
            return false;
        
        var changedEntities = await dataSet.SaveChangesAsync();
        return changedEntities > 0;
    }

    public async Task<Player?> GetPlayerById(Guid playerId)
    {
        var player = await dataSet.Set<Player>()
            .FindAsync(playerId);

        return player;
    }

    public async Task<bool> UpdatePlayerAsync(Player player)
    {
        var dbPlayer = await dataSet.Set<Player>()
            .FindAsync(player.Id);
        
        dataSet.Set<Player>()
            .Entry(dbPlayer)
            .CurrentValues.SetValues(player);
        
        var entry = dataSet.Set<Player>().Update(dbPlayer);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntities = await dataSet.SaveChangesAsync();
        return changedEntities > 0;
    }

    public async Task<bool> AddPlayerToRoomAsync(Guid playerId, Guid roomId)
    {
        var player = await GetPlayerById(playerId);
        var room = await dataSet.Set<GameRoom>().FindAsync(roomId);

        if (player is null || room is null)
            return false;

        room.Players.Add(player);

        var entry = dataSet.Set<GameRoom>().Update(room);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntities = await dataSet.SaveChangesAsync();
        return changedEntities > 0;
    }

    public async Task<bool> RemovePlayerFromRoomAsync(Guid playerId, Guid roomId)
    {
        var room = await dataSet.Set<GameRoom>().FindAsync(roomId);

        if (room is null)
            return false;

        room.Players.RemoveAll(p => Equals(p.Id, playerId));

        var entry = dataSet.Set<GameRoom>().Update(room);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntities = await dataSet.SaveChangesAsync();
        return changedEntities > 0;
    }
}
