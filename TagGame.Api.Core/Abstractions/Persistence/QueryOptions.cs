using System.Linq.Expressions;

namespace TagGame.Api.Core.Abstractions.Persistence;

public sealed class QueryOptions<T> where T : class
{
    public bool AsNoTracking { get; init; }

    public List<Expression<Func<T, object>>> Includes { get; } = [];

    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; init; }
}

