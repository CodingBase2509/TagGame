using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using TagGame.Client.Core.Services.Abstractions;

namespace TagGame.Client.Core.Http;

/// <summary>
/// Placeholder delegating handler that will attach Authorization headers
/// once the AuthService/TokenStorage (#54) is available. Currently a no-op.
/// Keep this handler first in the chain so downstream handlers see final headers.
/// </summary>
public sealed class AuthorizedHttpHandler(
    IAuthService authService,
    IAppPreferences preferences,
    ILogger<AuthorizedHttpHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Do not interfere with auth endpoints to avoid recursion/loops
        if (request.RequestUri is not null && IsAuthPath(request.RequestUri))
            return await base.SendAsync(request, cancellationToken);

        // Try to get a valid token; if missing/invalid, proactively attempt a refresh once
        var token = await authService.GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            var refreshed = await authService.RefreshAsync(cancellationToken);
            if (refreshed)
            {
                token = await authService.GetValidAccessTokenAsync(cancellationToken);
            }
            // If still no token, attempt a single login using stored DeviceId
            if (string.IsNullOrEmpty(token))
            {
                var deviceId = preferences.Snapshot.DeviceId;
                if (deviceId == Guid.Empty)
                {
                    deviceId = Guid.NewGuid();
                    await preferences.SetDeviceId(deviceId, cancellationToken);
                }
                var loggedIn = await authService.LoginAsync(deviceId.ToString("N"), cancellationToken);
                if (loggedIn)
                    token = await authService.GetValidAccessTokenAsync(cancellationToken);
            }
        }

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        // Attempt a single refresh on 401 (no auto-login here to avoid loops)
        logger.LogDebug("401 received, attempting token refresh and retry for {Method} {Uri}", request.Method, request.RequestUri);
        var postRefreshed = await authService.RefreshAsync(cancellationToken);
        if (!postRefreshed)
            return response;

        var newToken = await authService.GetValidAccessTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(newToken))
            return response;

        // Retry only idempotent/no-body methods to avoid side-effects
        if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head &&
            request.Method != HttpMethod.Delete)
            return response;

        var retry = new HttpRequestMessage(request.Method, request.RequestUri);
        foreach (var header in request.Headers)
        {
            retry.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        return await base.SendAsync(retry, cancellationToken);
    }

    private static bool IsAuthPath(Uri uri)
    {
        var p = uri.AbsolutePath;
        return p.StartsWith("/v1/auth/", StringComparison.OrdinalIgnoreCase);
    }
}
