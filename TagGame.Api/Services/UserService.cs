using System.Drawing;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Services;

public class UserService(IDataSet db)
{
    public async Task<User?> AddUserAsync(string username, Color avatarColor)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
            DefaultName = username,
            DefaultAvatarColor = avatarColor,
        };
        
        var entity = await db.Set<User>().AddAsync(user);
        if (entity.State != EntityState.Added)
            return null;
        
        var changedEntities = await db.SaveChangesAsync();
        return changedEntities == 0 ? null : user;
    }

    public async Task<bool> CheckIfUserExists(Guid userId)
    {
        var user = await db.Set<User>().FindAsync(userId);
        return user is null ? false : true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await db.Set<User>().FindAsync(userId);
        if (user is null)
            return false;
        
        var entity = db.Set<User>().Remove(user);
        if (entity.State != EntityState.Deleted)
            return false;
        
        var changedEntities = await db.SaveChangesAsync();
        return changedEntities > 0;
    }
}