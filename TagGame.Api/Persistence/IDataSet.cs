using TagGame.Shared.Domain.Common;

namespace TagGame.Api.Persistence;

public interface IDataSet<T> where T : class, IIdentifiable
{
    public Task<T?> GetByIdAsync(Guid id, bool isTracking = true);
    public IEnumerable<T> Where(Func<T, bool> predicate, bool isTracking = true);
    public Task<bool> AddAsync(T entity);
    public Task<bool> UpdateAsync(T entity);
    public Task<bool> DeleteAsync(T entity);
}