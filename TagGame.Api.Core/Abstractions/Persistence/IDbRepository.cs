using System.Linq.Expressions;

namespace TagGame.Api.Core.Abstractions.Persistence;

public interface IDbRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object[] keyValues, QueryOptions<T>? options = null, CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        QueryOptions<T>? options = null,
        CancellationToken ct = default);

    Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        QueryOptions<T>? options = null,
        CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    Task UpdateAsync(T entity, CancellationToken ct = default);

    Task DeleteAsync(T entity, CancellationToken ct = default);

    Task<uint> GetConcurrencyToken(T entity, CancellationToken ct = default);
}
