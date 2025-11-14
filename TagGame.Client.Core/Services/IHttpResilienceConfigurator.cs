using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Options;

namespace TagGame.Client.Core.Services;

/// <summary>
/// Configures the HttpClient resilience pipeline (retry/timeout) for a typed client.
/// </summary>
public interface IHttpResilienceConfigurator
{
    /// <summary>
    /// Applies retry/timeout configuration to the provided HttpClient builder using the supplied options.
    /// </summary>
    /// <param name="builder">The HttpClient builder to configure.</param>
    /// <param name="httpOptions">Resilience options (retries, delays, timeouts).</param>
    void Configure(IHttpClientBuilder builder, NetworkResilienceOptions.HttpOptions httpOptions);
}
