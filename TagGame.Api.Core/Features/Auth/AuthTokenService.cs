using Microsoft.Extensions.Options;
using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Api.Core.Features.Auth;

public class AuthTokenService(AuthDbContext db, IOptions<JwtOptions> jwtOptions, TimeProvider clock) : IAuthTokenService
{
    public async Task<TokenPairDto> IssueTokenAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (user.Id == Guid.Empty)
            throw new ArgumentException("User must have an Id", nameof(user));

        var now = clock.GetUtcNow();
        var (token, exp) = AuthTokenHelper.CreateJwtToken(user, now, jwtOptions.Value);
        var (refreshToken, hash, refreshExp, familyId) = AuthTokenHelper.CreateRefreshToken(null, now, jwtOptions.Value);

        var rt = new RefreshToken()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = hash,
            ExpiresAt = refreshExp,
            CreatedAt = now,
            ReplacedById = null,
            RevokedAt = null,
        };

        await db.RefreshTokens.AddAsync(rt, ct);
        await db.SaveChangesAsync(ct);

        return new TokenPairDto
        {
            AccessToken = token,
            AccessExpiresAt = exp,
            RefreshToken = refreshToken,
            RefreshExpiresAt = refreshExp,
        };
    }

    public async Task<TokenPairDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));

        var now = clock.GetUtcNow();
        var existing = await GetRefreshToken(refreshToken, ct);
        AuthTokenHelper.EnsureToken(existing, now);

        var user = await db.Users.FindAsync([existing!.UserId], ct)
                   ?? throw new InvalidOperationException("User not found for refresh token.");

        var (rawNew, newHash, newExp, _) = AuthTokenHelper.CreateRefreshToken(existing.FamilyId, now, jwtOptions.Value);

        var newRt = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            FamilyId = existing.FamilyId,
            TokenHash = newHash,
            CreatedAt = now,
            ExpiresAt = newExp
        };

        existing.RevokedAt = now;
        existing.ReplacedById = newRt.Id;

        db.RefreshTokens.Add(newRt);

        var (access, accessExp) = AuthTokenHelper.CreateJwtToken(user, now, jwtOptions.Value);

        await db.SaveChangesAsync(ct);

        return new TokenPairDto
        {
            AccessToken = access,
            AccessExpiresAt = accessExp,
            RefreshToken = rawNew,
            RefreshExpiresAt = newExp
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));

        var now = clock.GetUtcNow();
        var existing = await GetRefreshToken(refreshToken, ct);

        if (existing is null || existing.RevokedAt.HasValue)
            return;

        existing.RevokedAt = now;
        await db.SaveChangesAsync(ct);
    }

    private async Task<RefreshToken?> GetRefreshToken(string refreshToken, CancellationToken ct = default)
    {
        var hash = AuthTokenHelper.HashRefreshToken(refreshToken);

        return await db.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
    }
}
