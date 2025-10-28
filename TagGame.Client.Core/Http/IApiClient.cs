namespace TagGame.Client.Core.Http;

/// <summary>
/// Minimal typed API client for JSON-based HTTP operations.
/// Domain services can build on top of this interface.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends a GET request and deserializes a JSON response to the specified type.
    /// </summary>
    Task<T?> GetAsync<T>(string path, CancellationToken ct = default);

    /// <summary>
    /// Sends a POST request with a JSON body and deserializes the JSON response.
    /// Returns null for empty/204 responses.
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);

    /// <summary>
    /// Sends a PATCH request with a JSON body and deserializes the JSON response.
    /// Returns null for empty/204 responses.
    /// </summary>
    Task<TResponse?> PatchAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);

    /// <summary>
    /// Sends a DELETE request and ensures a successful status code.
    /// </summary>
    Task DeleteAsync(string path, CancellationToken ct = default);
}
