using System.Net.Http.Json;
using TagGame.Client.Core.Json;

namespace TagGame.Client.Core.Http;

/// <summary>
/// Delegating handler that maps non-success HTTP responses to ApiProblemException,
/// parsing application/problem+json payloads when available.
/// Place this handler outermost in the chain so retries/timeout happen before mapping.
/// </summary>
public sealed class ProblemDetailsHandler(IJsonOptionsProvider json) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return response;

        ApiProblemDetails? problem = null;
        try
        {
            var mediaType = response.Content?.Headers?.ContentType?.MediaType ?? string.Empty;
            if (mediaType.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase) ||
                mediaType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                problem = await response.Content!.ReadFromJsonAsync<ApiProblemDetails>(json.Options, cancellationToken);
            }
        }
        catch
        {
            // Swallow parse issues; we'll still throw with status/reason below
        }

        var message = problem?.Title ?? response.ReasonPhrase ?? "API request failed";
        throw new ApiProblemException(response.StatusCode, message, problem);
    }
}
