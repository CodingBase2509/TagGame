using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Services;

public class PlayerService(IDataAccess db)
{
    public async Task<Player?> CreatePlayerAsync(Guid userId)
    {
        var user = await db.Users.GetByIdAsync(userId, false);
        if (user is null)
            return null;
        
        var player = new Player()
        {
            Id = Guid.NewGuid(),
            AvatarColor = user.DefaultAvatarColor,
            UserName = user.DefaultName,
            UserId = user.Id,
        };
        
        var success = await db.Players.AddAsync(player);
        if (!success)
            return null;
        
        success = await db.SaveChangesAsync();
        return success ? player : null;
    }

    public async Task<bool> DeletePlayerAsync(Guid playerId)
    {
        if (Equals(playerId, Guid.Empty))
            return false;
        
        var player = await GetPlayerById(playerId);
        if (player is null)
            return false;
        
        var success = await db.Players
            .DeleteAsync(player);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }

    public async Task<Player?> GetPlayerById(Guid playerId)
    {
        var player = await db.Players
            .GetByIdAsync(playerId, false);

        return player;
    }

    public Task<Player?> GetPlayerByUserId(Guid userId)
    {
        var player = db.Players
            .Where(p => Equals(p.UserId, userId))
            .FirstOrDefault();
        
        return Task.FromResult(player);
    }

    public async Task<bool> UpdatePlayerAsync(Player player)
    {
        var success = await db.Players.UpdateAsync(player);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }

    public async Task<bool> AddPlayerToRoomAsync(Guid roomId, Guid playerId)
    {
        var player = await db.Players.GetByIdAsync(playerId);
        var room = await db.Rooms.GetByIdAsync(roomId);

        if (player is null || room is null)
            return false;

        room.Players.Add(player);

        var success = await db.Rooms.UpdateAsync(room);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }

    public async Task<bool> RemovePlayerFromRoomAsync(Guid playerId, Guid roomId)
    {
        var room = await db.Rooms.GetByIdAsync(roomId);

        if (room is null)
            return false;

        room.Players.RemoveAll(p => Equals(p.Id, playerId));

        var success = await db.Rooms.UpdateAsync(room);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }
}
