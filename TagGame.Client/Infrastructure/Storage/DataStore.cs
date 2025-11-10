using System.Text;
using System.Text.Json;
using TagGame.Client.Core.Json;
using TagGame.Client.Core.Storage;

namespace TagGame.Client.Infrastructure.Storage;

public class DataStore<T>(IProtectedStorage storage, IJsonOptionsProvider jsonOptions) : IDataStore<T>
{
    public async Task<T?> LoadAsync(CancellationToken ct = default)
    {
        const string fileName = nameof(T);
        var contentBytes = await storage.ReadAsync(fileName, ct);
        if (!contentBytes.HasValue)
            return default;

        var contentString = Encoding.UTF8.GetString(contentBytes.Value.Span);
        return JsonSerializer.Deserialize<T>(contentString, jsonOptions.Options);
    }

    public Task SaveAsync(T value, CancellationToken ct = default)
    {
        const string fileName = nameof(T);
        var contentString = JsonSerializer.Serialize(value, jsonOptions.Options);
        var contentBytes = Encoding.UTF8.GetBytes(contentString);
        return storage.WriteAsync(fileName, contentBytes, ct);
    }

    public Task ClearAsync()
    {
        const string fileName = nameof(T);
        return storage.DeleteAsync(fileName);
    }
}
