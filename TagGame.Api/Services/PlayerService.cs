using System.Drawing;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Services;

public class PlayerService
{
    private readonly IDataSet _db;

    public PlayerService(IDataSet dataSet)
    {
        this._db = dataSet;
    }

// --- Player-Management ---
    public async Task<Player?> CreatePlayerAsync(Guid userId)
    {
        var user = await _db.Set<User>().FindAsync(userId);
        if (user is null)
            return null;
        
        var player = new Player()
        {
            Id = user.Id,
            AvatarColor = user.DefaultAvatarColor,
            UserName = user.DefaultName,
        };
        
        var entity = await _db.Set<Player>().AddAsync(player);
        if (entity.State != EntityState.Added)
            return null;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities == 0 ? null : player;
    }

    public async Task<bool> DeletePlayerAsync(Guid playerId)
    {
        if (Equals(playerId, Guid.Empty))
            return false;
        
        var player = await GetPlayerById(playerId);
        if (player is null)
            return false;
        
        var entity = _db.Set<Player>()
            .Remove(player);
        if (entity.State != EntityState.Deleted)
            return false;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities > 0;
    }

    public async Task<Player?> GetPlayerById(Guid playerId)
    {
        var player = await _db.Set<Player>()
            .FindAsync(playerId);

        return player;
    }

    public async Task<bool> UpdatePlayerAsync(Player player)
    {
        var dbPlayer = await _db.Set<Player>()
            .FindAsync(player.Id);
        
        _db.Set<Player>()
            .Entry(dbPlayer)
            .CurrentValues.SetValues(player);
        
        var entry = _db.Set<Player>().Update(dbPlayer);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities > 0;
    }

    public async Task<bool> AddPlayerToRoomAsync(Guid playerId, Guid roomId)
    {
        var player = await GetPlayerById(playerId);
        var room = await _db.Set<GameRoom>().FindAsync(roomId);

        if (player is null || room is null)
            return false;

        room.Players.Add(player);

        var entry = _db.Set<GameRoom>().Update(room);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities > 0;
    }

    public async Task<bool> RemovePlayerFromRoomAsync(Guid playerId, Guid roomId)
    {
        var room = await _db.Set<GameRoom>().FindAsync(roomId);

        if (room is null)
            return false;

        room.Players.RemoveAll(p => Equals(p.Id, playerId));

        var entry = _db.Set<GameRoom>().Update(room);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities > 0;
    }

// --- User-Management ---
    public async Task<User?> AddUserAsync(string username, Color avatarColor)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
            DefaultName = username,
            DefaultAvatarColor = avatarColor,
        };
        
        var entity = await _db.Set<User>().AddAsync(user);
        if (entity.State != EntityState.Added)
            return null;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities == 0 ? null : user;
    }

    public async Task<bool> CheckIfUserExists(Guid userId)
    {
        var user = await _db.Set<User>().FindAsync(userId);
        return user is null ? false : true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _db.Set<User>().FindAsync(userId);
        if (user is null)
            return false;
        
        var entity = _db.Set<User>().Remove(user);
        if (entity.State != EntityState.Deleted)
            return false;
        
        var changedEntities = await _db.SaveChangesAsync();
        return changedEntities > 0;
    }
}
