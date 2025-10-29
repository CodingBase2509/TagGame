using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Abstractions.Persistence;

public interface IAuthUoW : IUnitOfWork
{
    IDbRepository<User> Users { get; }

    IDbRepository<RefreshToken> RefreshTokens { get; }

    IDbRepository<Entitlement> Entitlements { get; }
}
