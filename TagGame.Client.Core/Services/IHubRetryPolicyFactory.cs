using Microsoft.AspNetCore.SignalR.Client;
using TagGame.Client.Core.Options;

namespace TagGame.Client.Core.Services;

/// <summary>
/// Creates SignalR reconnect retry policies based on configured delays/options.
/// </summary>
public interface IHubRetryPolicyFactory
{
    /// <summary>
    /// Creates a retry policy using the provided hub options (reconnect delays, max window).
    /// </summary>
    /// <param name="hubOptions">Reconnect delays and optional max window.</param>
    IRetryPolicy Create(NetworkResilienceOptions.HubOptions hubOptions);
}
