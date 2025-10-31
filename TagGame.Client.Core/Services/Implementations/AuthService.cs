using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TagGame.Client.Core.Http;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;
using TagGame.Client.Core.Storage;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Client.Core.Services.Implementations;

public class AuthService(
    IApiClient api,
    ITokenStorage tokenStorage,
    IAppPreferences preferences,
    IOptions<AuthServiceOptions> options,
    TimeProvider clock,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<bool> IsLoggedInAsync(CancellationToken ct = default)
    {
        var tokenPair = await tokenStorage.GetAsync(ct);
        if (tokenPair is null) return false;
        var now = clock.GetUtcNow();
        return tokenPair.RefreshExpiresAt > now;
    }

    public async Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default)
    {
        var t = await tokenStorage.GetAsync(ct);
        if (t is not null && !AccessExpired(t))
            return t.AccessToken;

        var ok = await RefreshAsync(CreateRefreshCt(ct));
        if (!ok)
            return null;

        t = await tokenStorage.GetAsync(ct);
        return t?.AccessToken;
    }

    public async Task SetTokensAsync(TokenPairDto tokenPair, CancellationToken ct = default) =>
        await tokenStorage.SetAsync(tokenPair, ct);

    public async Task<bool> RefreshAsync(CancellationToken ct = default)
    {
        var t = await tokenStorage.GetAsync(ct);
        if (t is null) return false;

        if (t.RefreshExpiresAt <= clock.GetUtcNow())
        {
            await tokenStorage.ClearAsync(ct);
            return false;
        }

        try
        {
            var data = new RefreshRequestDto { RefreshToken = t.RefreshToken };
            var resp = await api.PostAsync<RefreshRequestDto, RefreshResponseDto>(options.Value.RefreshPath, data, ct);
            if (resp?.Tokens is null)
            {
                await tokenStorage.ClearAsync(ct);
                return false;
            }

            await SetTokensAsync(resp.Tokens, ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token refresh failed");
            await tokenStorage.ClearAsync(ct);
            return false;
        }
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            var t = await tokenStorage.GetAsync(ct);
            if (t is null)
            {
                await tokenStorage.ClearAsync(ct);
                return;
            }

            var resp = await api.PostAsync<LogoutRequestDto, LogoutResponseDto>(options.Value.LogoutPath,
                new LogoutRequestDto { RefreshToken = t.RefreshToken }, ct);

            await tokenStorage.ClearAsync(ct);
            await preferences.SetUserId(Guid.Empty, ct);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Logout request failed; tokens cleared locally");
            await tokenStorage.ClearAsync(ct);
        }
    }

    public async Task<bool> LoginAsync(string deviceId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("DeviceId required", nameof(deviceId));

        try
        {
            var data = new LoginRequestDto { DeviceId = deviceId };
            var resp = await api.PostAsync<LoginRequestDto, LoginResponseDto>("/v1/auth/login", data, ct);
            if (resp is null)
                return false;

            await SetTokensAsync(resp.Tokens, ct);
            await preferences.SetUserId(resp.UserId, ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to login.");
            return false;
        }
    }

    public async Task<bool> InitialAsync(string deviceId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId required", nameof(deviceId));

        try
        {
            var data = new InitialRequestDto { DeviceId = deviceId };
            var resp = await api.PostAsync<InitialRequestDto, InitialResponseDto>("/v1/auth/initial", data, ct);
            if (resp is null)
                return false;

            await SetTokensAsync(resp.Tokens, ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initial-register user.");
            return false;
        }
    }

    public Task UpgradeToFullAccountAsync(string email, CancellationToken ct = default) => throw new NotImplementedException();

    private bool AccessExpired(TokenPairDto tokenPair)
    {
        var now = clock.GetUtcNow();
        return tokenPair.AccessExpiresAt <= now + options.Value.AccessTokenRefreshSkew;
    }

    private CancellationToken CreateRefreshCt(CancellationToken ct)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(options.Value.RefreshRequestTimeout);
        return cts.Token;
    }
}
