using TagGame.Shared.DTOs.Auth;

namespace TagGame.Client.Core.Storage;

public interface ITokenStorage
{
    Task<TokenPairDto?> GetAsync(CancellationToken ct = default);

    Task SetAsync(TokenPairDto tokenPair, CancellationToken ct = default);

    Task ClearAsync(CancellationToken ct = default);

    event EventHandler<TokenPairDto?> TokensChanged;
}
