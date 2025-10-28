namespace TagGame.Client.Core.Http;

/// <summary>
/// Placeholder delegating handler that will attach Authorization headers
/// once the AuthService/TokenStorage from #54 is available.
/// Currently, it passes requests through unchanged.
/// </summary>
public sealed class AuthorizedHttpHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Future: inject token provider and attach Bearer token here.
        return base.SendAsync(request, cancellationToken);
    }
}

