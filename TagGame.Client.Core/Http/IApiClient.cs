namespace TagGame.Client.Core.Http;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken ct = default);

    Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);

    Task<TResponse?> PatchAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);

    Task DeleteAsync(string path, CancellationToken ct = default);
}
