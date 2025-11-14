using System.Text;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Storage;
using TagGame.Client.Extensions;

namespace TagGame.Client.Infrastructure.Storage;

public class ProtectedStorage(ISecureStorage storage, ICrypto crypto, ILogger logger) : IProtectedStorage
{
    private const string KeyName = "files.encryption";
    private const string Extension = ".dat";
    private const string AadPrefix = "tg.local/v1|";
    private static readonly string AppDataPath = FileSystem.AppDataDirectory;

    private static string FileName(string name) => Path.GetFileName(name) + Extension;
    private static byte[] Aad(string name) => Encoding.UTF8.GetBytes(AadPrefix + name);

    public async Task<ReadOnlyMemory<byte>?> ReadAsync(string name, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        byte[]? blob = null;
        try
        {
            blob = await File.ReadAllBytesAsync(Path.Combine(AppDataPath, FileName(name)), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ProtectedStorage failed to read {Name}: {Message}", name, ex.Message);
        }

        if (blob is null)
            return null;

        var key = await storage.GetOrCreateKeyAsync(KeyName);
        var aad = Aad(name);
        var clearText = await crypto.Decrypt(Encoding.UTF8.GetBytes(key), blob, aad);
        logger.LogDebug("ProtectedStorage read {Name} ({Length} bytes)", name, clearText.Length);
        return new ReadOnlyMemory<byte>(clearText);
    }

    public async Task WriteAsync(string name, ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var key = await storage.GetOrCreateKeyAsync(KeyName);
        var aad = Aad(name);

        var blob = await crypto.Encrypt(Encoding.UTF8.GetBytes(key), data.ToArray(), aad);
        await File.WriteAllBytesAsync(Path.Combine(AppDataPath, FileName(name)), blob, ct);
        logger.LogDebug("ProtectedStorage wrote {Name} ({Length} bytes)", name, data.Length);
    }

    public Task DeleteAsync(string name, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var path = Path.Combine(AppDataPath, FileName(name));
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }
}
