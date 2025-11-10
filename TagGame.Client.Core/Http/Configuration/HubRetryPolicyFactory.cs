using Microsoft.AspNetCore.SignalR.Client;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Http.Configuration;

/// <summary>
/// Creates a fixed-delays reconnect policy for SignalR based on configured hub options.
/// </summary>
public sealed class HubRetryPolicyFactory : IHubRetryPolicyFactory
{
    public IRetryPolicy Create(NetworkResilienceOptions.HubOptions hub)
    {
        var delays = hub.ReconnectDelaysMs
            .Select(ms => TimeSpan.FromMilliseconds(ms))
            .ToArray();

        var maxWindow = hub.MaxReconnectWindowMs > 0
            ? TimeSpan.FromMilliseconds(hub.MaxReconnectWindowMs)
            : (TimeSpan?)null;

        return new FixedDelaysRetryPolicy(delays, maxWindow);
    }

    /// <summary>
    /// Simple retry policy that yields the configured delay sequence and stops after an optional window.
    /// </summary>
    private sealed class FixedDelaysRetryPolicy(TimeSpan[] delays, TimeSpan? maxWindow) : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            if (maxWindow is { } window && retryContext.ElapsedTime > window)
                return null;

            var index = retryContext.PreviousRetryCount;
            return index < delays.Length ? delays[index] : null;
        }
    }
}
