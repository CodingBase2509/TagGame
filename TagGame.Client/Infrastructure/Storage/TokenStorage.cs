using System.Text.Json;
using TagGame.Client.Core.Storage;
using TagGame.Shared.DTOs.Auth;
using TagGame.Shared.Json;

namespace TagGame.Client.Infrastructure.Storage;

public class TokenStorage(ISecureStorage storage, ILogger<TokenStorage> logger) : ITokenStorage, IDisposable
{
    private const string StorageKey = "auth.tokens";
    private readonly SemaphoreSlim _gate = new(1, 1);
    private TokenPairDto? _cache;

    public event EventHandler<TokenPairDto?>? TokensChanged;

    public async Task<TokenPairDto?> GetAsync(CancellationToken ct = default)
    {
        if (_cache is not null)
            return _cache;

        var raw = await storage.GetAsync(StorageKey);
        return string.IsNullOrWhiteSpace(raw) ?
            null
            : JsonSerializer.Deserialize<TokenPairDto>(raw, JsonDefaults.Options);
    }

    public async Task SetAsync(TokenPairDto tokenPair, CancellationToken ct = default)
    {
        if (tokenPair is null)
            return;

        try
        {
            await _gate.WaitAsync(ct);
            _cache = tokenPair;
            var raw = JsonSerializer.Serialize(tokenPair, JsonDefaults.Options);
            await storage.SetAsync(StorageKey, raw);

            TokensChanged?.Invoke(this, tokenPair);
        }
        catch (Exception e)
        {
            logger.LogWarning("Failed to store tokens: ${Message}", e.Message);
        }
        finally
        {
            _gate.Release();
        }

    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        try
        {
            await _gate.WaitAsync(ct);
            _cache = null;
            storage.Remove(StorageKey);

            TokensChanged?.Invoke(this, null);
        }
        catch (Exception e)
        {
            logger.LogWarning("Failed to remove tokens: ${Message}", e.Message);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _gate.Dispose();
    }
}
