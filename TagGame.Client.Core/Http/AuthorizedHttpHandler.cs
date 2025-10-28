namespace TagGame.Client.Core.Http;

/// <summary>
/// Placeholder delegating handler that will attach Authorization headers
/// once the AuthService/TokenStorage (#54) is available. Currently a no-op.
/// Keep this handler first in the chain so downstream handlers see final headers.
/// </summary>
public sealed class AuthorizedHttpHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Future: inject token provider and attach Bearer token here.
        return base.SendAsync(request, cancellationToken);
    }
}
