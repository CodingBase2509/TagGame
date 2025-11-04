using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Persistence.Repositories;

public sealed class EfDbRepository<T> : IDbRepository<T> where T : class
{
    private readonly DbContext db;

    public EfDbRepository(IServiceProvider serviceProvider)
    {
        IEnumerable<Type> authTypes = [typeof(User), typeof(RefreshToken), typeof(Entitlement)];
        db = authTypes.Contains(typeof(T))
            ? serviceProvider.GetRequiredService<AuthDbContext>()
            : serviceProvider.GetRequiredService<GamesDbContext>();
    }

    public async Task<T?> GetByIdAsync(object[] keyValues, QueryOptions<T>? options = null, CancellationToken ct = default)
    {
        // Build a predicate based on the primary key metadata to allow Includes/AsNoTracking
        var entityType = db.Model.FindEntityType(typeof(T))
                         ?? throw new InvalidOperationException($"No entity type metadata for {typeof(T).Name}.");
        var key = entityType.FindPrimaryKey()
                  ?? throw new InvalidOperationException($"Entity {typeof(T).Name} has no primary key defined.");
        if (key.Properties.Count != keyValues.Length)
            throw new ArgumentException($"Key value count mismatch for {typeof(T).Name}. Expected {key.Properties.Count}, got {keyValues.Length}.");

        var param = Expression.Parameter(typeof(T), "e");
        Expression? predicate = null;
        for (var i = 0; i < key.Properties.Count; i++)
        {
            var prop = key.Properties[i];
            var efProperty = typeof(EF).GetMethod(nameof(EF.Property))!
                .MakeGenericMethod(prop.ClrType);
            var left = Expression.Call(efProperty, param, Expression.Constant(prop.Name));
            var right = Expression.Convert(Expression.Constant(keyValues[i]), prop.ClrType);
            var eq = Expression.Equal(left, right);
            predicate = predicate is null ? eq : Expression.AndAlso(predicate, eq);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(predicate!, param);
        return await FirstOrDefaultAsync(lambda, options, ct);
    }
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        QueryOptions<T>? options = null,
        CancellationToken ct = default)
    {
        var query = ApplyOptions(db.Set<T>().AsQueryable(), options);
        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        QueryOptions<T>? options = null,
        CancellationToken ct = default)
    {
        var query = ApplyOptions(db.Set<T>().AsQueryable(), options);
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.ToListAsync(ct);
    }

    public Task AddAsync(T entity, CancellationToken ct = default) =>
        db.Set<T>().AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // If an instance with same key is already tracked, copy values into it to avoid tracking conflicts
        var local = FindTrackedByKey(entity);
        if (local is not null)
        {
            db.Entry(local).CurrentValues.SetValues(entity);
        }
        else
        {
            db.Set<T>().Update(entity);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        db.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<uint> GetConcurrencyToken(T entity, CancellationToken ct = default)
    {
        await db.Entry(entity).ReloadAsync(ct);
        return db.Entry(entity).Property<uint>("xmin").CurrentValue;
    }

    private static IQueryable<T> ApplyOptions(IQueryable<T> query, QueryOptions<T>? options)
    {
        if (options is null)
            return query.AsTracking();

        if (options.AsNoTracking)
            query = query.AsNoTracking();

        query = options.Includes.
            Aggregate(query, (current, include) =>
                current.Include(include));

        if (options.OrderBy is not null)
            query = options.OrderBy(query);

        return query;
    }

    private T? FindTrackedByKey(T entity)
    {
        var entityType = db.Model.FindEntityType(typeof(T));
        var key = entityType?.FindPrimaryKey();

        if (key is null)
            return null;

        foreach (var tracked in db.Set<T>().Local)
        {
            var match = true;
            foreach (var prop in key.Properties)
            {
                var trackedVal = db.Entry(tracked).Property(prop.Name).CurrentValue;
                var newVal = db.Entry(entity).Property(prop.Name).CurrentValue;
                if (Equals(trackedVal, newVal))
                    continue;

                match = false;
                break;
            }
            if (match)
                return tracked;
        }

        return null;
    }
}
