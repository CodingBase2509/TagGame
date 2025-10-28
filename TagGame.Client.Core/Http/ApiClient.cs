using System.Net.Http.Json;
using TagGame.Client.Core.Json;

namespace TagGame.Client.Core.Http;

/// <summary>
/// Default implementation of the typed API client using HttpClient and shared JSON options.
/// </summary>
public sealed class ApiClient(HttpClient client, IJsonOptionsProvider jsonProvider) : IApiClient
{
    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default) =>
        await client.GetFromJsonAsync<T>(path, jsonProvider.Options, ct);

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        var res = await client.PostAsJsonAsync(path, body, jsonProvider.Options, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TResponse>(jsonProvider.Options, ct);
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Patch, path);
        req.Content = JsonContent.Create(body, options: jsonProvider.Options);

        var resp = await client.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TResponse>(jsonProvider.Options, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var resp = await client.DeleteAsync(path, ct);
        resp.EnsureSuccessStatusCode();
    }
}
