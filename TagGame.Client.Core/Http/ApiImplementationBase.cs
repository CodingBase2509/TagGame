using TagGame.Client.Core.Services;
using TagGame.Client.Core.Storage;

namespace TagGame.Client.Core.Http;

public abstract class ApiImplementationBase
{
    public async Task EnsureValidToken(CancellationToken ct = default)
    {
        var storage = SpUtils.GetRequiredService<ITokenStorage>();
        var tokens = await storage.GetAsync(ct);

        if (tokens is null || !IsTokenValid(tokens.RefreshExpiresAt))
            await LoginAsync(ct);

        if (!IsTokenValid(tokens!.AccessExpiresAt))
            await RefreshAsync(ct);
    }

    private bool IsTokenValid(DateTimeOffset expiresAt) => expiresAt > DateTimeOffset.UtcNow;

    private async Task LoginAsync(CancellationToken ct)
    {
        var auth = SpUtils.GetRequiredService<IAuthService>();
        var prefs = SpUtils.GetRequiredService<IAppPreferences>();

        _ = await auth.LoginAsync(prefs.Snapshot.DeviceId, ct);
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        var auth = SpUtils.GetRequiredService<IAuthService>();
        _ = await auth.RefreshAsync(ct);
    }
}
