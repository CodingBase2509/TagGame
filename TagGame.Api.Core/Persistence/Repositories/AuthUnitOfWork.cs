using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Persistence.Repositories;

public class AuthUnitOfWork(AuthDbContext db, IServiceProvider serviceProvider) : IAuthUoW
{
    public IDbRepository<User> Users => GetRepository<User>();

    public IDbRepository<RefreshToken> RefreshTokens => GetRepository<RefreshToken>();

    public IDbRepository<Entitlement> Entitlements => GetRepository<Entitlement>();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);

    private IDbRepository<T> GetRepository<T>() where T : class =>
        serviceProvider.GetRequiredService<IDbRepository<T>>();
}
