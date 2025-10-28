using TagGame.Shared.Domain.Auth;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Api.Core.Abstractions.Auth;

public interface IAuthTokenService
{
    Task<TokenPairDto> IssueTokenAsync(User user, CancellationToken ct = default);
    Task<TokenPairDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}
