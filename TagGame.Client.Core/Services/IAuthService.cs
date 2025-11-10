using TagGame.Shared.DTOs.Auth;

namespace TagGame.Client.Core.Services;

public interface IAuthService
{
    Task<bool> IsLoggedInAsync(CancellationToken ct = default);

    Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default);

    Task<bool> RefreshAsync(CancellationToken ct = default);

    Task SetTokensAsync(TokenPairDto tokenPair, CancellationToken ct = default);

    Task LogoutAsync(CancellationToken ct = default);

    Task<bool> LoginAsync(string deviceId, CancellationToken ct = default);

    Task<bool> InitialAsync(string deviceId, CancellationToken ct = default);

    Task UpgradeToFullAccountAsync(string email, CancellationToken ct = default);
}
