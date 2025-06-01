using System.Text.Json;

namespace TagGame.Client;

public class DebugSecureStorage : ISecureStorage
{
    private static string _fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "securestorage.json");
    private static readonly SemaphoreSlim _lock = new(1, 1);
    
    public async Task<string?> GetAsync(string key)
    {
        await _lock.WaitAsync();
        try
        {
            var dict = await ReadAsync();
            return dict.TryGetValue(key, out var value) ? value : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync(string key, string value)
    {
        await _lock.WaitAsync();
        try
        {
            var dict = await ReadAsync();
            dict[key] = value;
            await WriteAsync(dict);
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool Remove(string key)
    {
        _lock.Wait();
        try
        {
            var dict = ReadAsync().Result;
            var removed = dict.Remove(key);
            if (removed)
                WriteAsync(dict).Wait();
            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void RemoveAll()
    {
        _lock.Wait();
        try
        {
            if (File.Exists(_fileName))
                File.Delete(_fileName);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static async Task<Dictionary<string, string>> ReadAsync()
    {
        if (!File.Exists(_fileName))
            return new Dictionary<string, string>();

        var json = await File.ReadAllTextAsync(_fileName);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
    }

    private static async Task WriteAsync(Dictionary<string, string> data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_fileName, json);
    }
}