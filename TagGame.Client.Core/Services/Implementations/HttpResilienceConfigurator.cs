using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;

namespace TagGame.Client.Core.Services.Implementations;

public class HttpResilienceConfigurator : IHttpResilienceConfigurator
{
    public void Configure(IHttpClientBuilder builder, NetworkResilienceOptions.HttpOptions httpOptions) =>
        builder.AddResilienceHandler("default", p =>
            BuildPipeline(p, httpOptions));

    private static void BuildPipeline(
        ResiliencePipelineBuilder<HttpResponseMessage> pipeline,
        NetworkResilienceOptions.HttpOptions http)
    {
        pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = http.MaxRetries,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(http.BaseDelayMs),

            // 5xx, 408, HttpRequestException; 429 optional
            ShouldHandle = args =>
            {
                if (args.Outcome.Exception is HttpRequestException or TaskCanceledException)
                    return ValueTask.FromResult(true);

                if (args.Outcome.Result is not { } resp)
                    return ValueTask.FromResult(false);

                var status = resp.StatusCode;
                var is5Xx = status is >= (HttpStatusCode)500 and <= (HttpStatusCode)599 and not HttpStatusCode.NotImplemented;
                var is408 = status is HttpStatusCode.RequestTimeout;
                var isTransientStatus = is5Xx || is408;
                var is429 = http.RetryOn429 && (int)status == 429;
                return ValueTask.FromResult(isTransientStatus || is429);
            },

            // Optional: Retry-After berücksichtigen (gekappt per MaxDelayMs)
            DelayGenerator = async args =>
            {
                if (args.Outcome.Result is not { } resp || resp.Headers.RetryAfter is not { } ra)
                    return await ValueTask.FromResult<TimeSpan?>(null);

                var delay = ra.Delta ?? TimeSpan.FromMilliseconds(http.BaseDelayMs);
                var cappedMs = Math.Min(delay.TotalMilliseconds, http.MaxDelayMs);
                return await ValueTask.FromResult<TimeSpan?>(TimeSpan.FromMilliseconds(cappedMs));
            }
        });

        // Total-Timeout (pro Request inkl. Retries)
        pipeline.AddTimeout(new HttpTimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromMilliseconds(http.TotalRequestTimeoutMs)
        });
    }

}
