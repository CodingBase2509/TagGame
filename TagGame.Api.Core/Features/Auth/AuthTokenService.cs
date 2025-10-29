using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Common.Exceptions;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Api.Core.Features.Auth;

public class AuthTokenService(
    IAuthUoW uow,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider clock,
    ILogger<AuthTokenService> logger) : IAuthTokenService
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

        await uow.RefreshTokens.AddAsync(rt, ct);
        await uow.SaveChangesAsync(ct);

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
        if (existing?.ReplacedById.HasValue ?? false)
        {
            await RevokeFamilyAsync(existing.FamilyId, now, ct);
            logger.LogWarning("Refresh token reuse detected: UserId={UserId}, TokenId={TokenId}, FamilyId={FamilyId}",
                existing.UserId, existing.Id, existing.FamilyId);
            throw new RefreshTokenReuseException(existing.UserId, existing.Id, existing.FamilyId);
        }
        AuthTokenHelper.EnsureToken(existing, now);

        var user = await uow.Users.GetByIdAsync([existing!.UserId], null, ct)
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

        await uow.RefreshTokens.AddAsync(newRt, ct);

        var (access, accessExp) = AuthTokenHelper.CreateJwtToken(user, now, jwtOptions.Value);

        await uow.SaveChangesAsync(ct);

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
        await uow.SaveChangesAsync(ct);
    }

    private async Task<RefreshToken?> GetRefreshToken(string refreshToken, CancellationToken ct = default)
    {
        var hash = AuthTokenHelper.HashRefreshToken(refreshToken);

        return await uow.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, new()
            {
                AsNoTracking = false
            }, ct);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken ct = default)
    {
        var tokens = await uow.RefreshTokens
            // .Where(t => t.FamilyId == familyId && !t.RevokedAt.HasValue)
            .ListAsync(t => t.FamilyId == familyId && !t.RevokedAt.HasValue, new()
            {
                AsNoTracking = false
            }, ct);

        foreach (var token in tokens)
            token.RevokedAt = now;

        await uow.SaveChangesAsync(ct);
    }
}
