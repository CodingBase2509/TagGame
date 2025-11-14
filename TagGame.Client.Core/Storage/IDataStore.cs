namespace TagGame.Client.Core.Storage;

public interface IDataStore<T>
{
    Task<T?> LoadAsync(CancellationToken ct = default);

    Task SaveAsync(T value, CancellationToken ct = default);

    Task ClearAsync();
}
