namespace TagGame.Client.Core.Options;

/// <summary>
/// Resilience configuration for HTTP and SignalR connectivity in the client.
/// Defaults are conservative and can be overridden via configuration (see appsettings).
/// </summary>
public sealed class NetworkResilienceOptions
{
    public HttpOptions Http { get; init; } = new();
    public HubOptions Hub { get; init; } = new();

    public sealed class HttpOptions
    {
        /// <summary>
        /// Maximum retry attempts for transient errors (5xx, 408, network errors). Does not include the initial try.
        /// </summary>
        public int MaxRetries { get; init; } = 3;

        /// <summary>
        /// Base delay in milliseconds for exponential backoff between retries.
        /// </summary>
        public int BaseDelayMs { get; init; } = 200;

        /// <summary>
        /// Maximum delay cap in milliseconds between retries (also used to cap Retry-After headers when present).
        /// </summary>
        public int MaxDelayMs { get; init; } = 2000;

        /// <summary>
        /// Total request timeout across all tries in milliseconds. Includes retries.
        /// </summary>
        public int TotalRequestTimeoutMs { get; init; } = 20_000;

        /// <summary>
        /// Retry HTTP 429 responses (Too Many Requests). If true, honors Retry-After when provided.
        /// </summary>
        public bool RetryOn429 { get; init; }

        /// <summary>
        /// Allow retries for non-idempotent methods (POST/PUT/PATCH/DELETE). Defaults to false.
        /// </summary>
        public bool RetryOnNonIdempotent { get; init; }
    }

    public sealed class HubOptions
    {
        /// <summary>
        /// Reconnect delays in milliseconds for SignalR automatic reconnect.
        /// </summary>
        public int[] ReconnectDelaysMs { get; init; } = [0, 1000, 2000, 5000, 10_000, 30_000];

        /// <summary>
        /// Maximum reconnect window in milliseconds before giving up (optional).
        /// </summary>
        public int MaxReconnectWindowMs { get; init; } = 60_000;
    }
}
