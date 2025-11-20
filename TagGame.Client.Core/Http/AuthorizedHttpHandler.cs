using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using TagGame.Client.Core.Storage;

namespace TagGame.Client.Core.Http;

/// <summary>
/// Delegating handler that attaches bearer tokens from storage. Token refresh/login
/// happens via AuthService on-demand; this handler only enriches outgoing requests.
/// Keep this handler first in the chain so downstream handlers see final headers.
/// </summary>
public sealed class AuthorizedHttpHandler(
    ITokenStorage tokenStorage,
    ILogger<AuthorizedHttpHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var tokens = await tokenStorage.GetAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(tokens?.AccessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to attach bearer token");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
