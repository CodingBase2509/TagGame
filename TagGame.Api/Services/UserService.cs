using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Common;

namespace TagGame.Api.Services;

public class UserService(IDataAccess db)
{
    public async Task<User?> AddUserAsync(string username, ColorDTO avatarColor)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
            DefaultName = username,
            DefaultAvatarColor = avatarColor,
        };
        
        var success = await db.Users.AddAsync(user);
        if (!success)
            return null;

        success = await db.SaveChangesAsync();
        return success ? user : null;
    }

    public async Task<bool> CheckIfUserExists(Guid userId)
    {
        var user = await db.Users.GetByIdAsync(userId, false);
        return user is null ? false : true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await db.Users.GetByIdAsync(userId, false);
        if (user is null)
            return false;
        
        var success = await db.Users.DeleteAsync(user);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }
}