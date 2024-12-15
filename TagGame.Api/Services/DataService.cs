using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Services;

public class DataService(GamesDbContext context) : IDataAccess
{
    public IDataSet<GameRoom> Rooms => new DataService<GameRoom>(context.Rooms);
    public IDataSet<GameSettings> Settings => new DataService<GameSettings>(context.Settings);
    public IDataSet<Player> Players => new DataService<Player>(context.Players);
    public IDataSet<User> Users => new DataService<User>(context.Users);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var changedEntitesCount = await context.SaveChangesAsync(cancellationToken);
            return changedEntitesCount > 0;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}

file class DataService<T>(DbSet<T> entities) : IDataSet<T> where T : class, IIdentifiable
{
    public async Task<T?> GetByIdAsync(Guid id, bool isTracking = true)
    {
        if (isTracking)
            return await entities.FindAsync(id);
        else
            return await entities
                .AsNoTracking()
                .Where(x => Equals(x.Id, id))
                .FirstOrDefaultAsync();
    }

    public IEnumerable<T> Where(Func<T, bool> predicate, bool isTracking = true)
    {
        var set = isTracking ? entities.AsQueryable() 
            : entities.AsNoTracking();
        
        return set
            .Where(predicate);
    }
    
    public async Task<bool> AddAsync(T entity)
    {
        var entry = await entities.AddAsync(entity);
        return entry.State == EntityState.Added;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var dbEntity = await entities.FindAsync(entity.Id);
        entities.Entry(dbEntity).CurrentValues.SetValues(entity);
        var entry = entities.Update(entity);
        return entry.State == EntityState.Modified;
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        var dbEntity = await entities.FindAsync(entity.Id);
        var entry = entities.Remove(dbEntity);
        return entry.State == EntityState.Deleted;
    }
}