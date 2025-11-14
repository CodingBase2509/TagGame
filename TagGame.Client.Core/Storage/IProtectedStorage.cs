namespace TagGame.Client.Core.Storage;

public interface IProtectedStorage
{
    Task<ReadOnlyMemory<byte>?> ReadAsync(string name, CancellationToken ct = default);

    Task WriteAsync(string name, ReadOnlyMemory<byte> data, CancellationToken ct = default);

    Task DeleteAsync(string name, CancellationToken ct = default);
}
