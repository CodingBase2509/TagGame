namespace TagGame.Client.Extensions;

public static class SecureStorageExtensions
{
    /// <summary>
    /// Returns the key-value from the storage key, if not exists creates a 265-bit key and returns it.
    /// </summary>
    /// <param name="storage">The secure storage instance.</param>
    /// <param name="key">The storage key.</param>
    /// <returns>The existing or newly created key.</returns>
    public static async Task<string> GetOrCreateKeyAsync(this ISecureStorage storage, string key)
    {
        var value = await storage.GetAsync(key);
        if (value is not null)
        {
            return value;
        }

        var newKey = Guid.NewGuid() + Guid.NewGuid().ToString();
        await storage.SetAsync(key, newKey);
        return newKey;
    }
}
