using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Persistence.Repositories;

public class GamesUnitOfWork(GamesDbContext db, IServiceProvider serviceProvider) : IGamesUoW
{
    public IDbRepository<GameRoom> Rooms => GetRepository<GameRoom>();
    public IDbRepository<RoomMembership> RoomMemberships => GetRepository<RoomMembership>();
    public IDbRepository<Match> Matches => GetRepository<Match>();
    public IDbRepository<Round> Rounds => GetRepository<Round>();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);

    private IDbRepository<T> GetRepository<T>() where T : class =>
        serviceProvider.GetRequiredService<IDbRepository<T>>();
}
