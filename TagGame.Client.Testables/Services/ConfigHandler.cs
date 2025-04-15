using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Maui.Storage;
using TagGame.Shared.Constants;

namespace TagGame.Client.Services;

public class ConfigHandler (Encryption crypt, ISecureStorage secureStorage, string configDir)
{
    private const string encryptedFileExtension = ".enc";
    private const string storageKey = "config_crypt";
    private readonly Dictionary<Type, ConfigBase> cachedConfigs = [];

    private readonly Encryption _crypt = crypt is not null ? crypt.WithStorageKey(storageKey) : crypt;

    public bool CanInteractWithFiles => _crypt.HasKeysLoaded;

    public ConfigHandler()
        : this(null, null, string.Empty)
    { }
    
    public async Task InitAsync()
    {
        // generate encryption key
        var aes = Aes.Create();
        await secureStorage.SetAsync(storageKey + "_key", JsonSerializer.Serialize(aes.Key, MappingOptions.JsonSerializerOptions));
        await secureStorage.SetAsync(storageKey + "_iv", JsonSerializer.Serialize(aes.IV, MappingOptions.JsonSerializerOptions));
    }
    
    public virtual async Task WriteAsync<TConfig>(TConfig config) where TConfig : ConfigBase
    {
        if (!cachedConfigs.ContainsKey(config.GetType()))
            cachedConfigs.TryAdd(typeof(TConfig), config);
        else
            cachedConfigs[typeof(TConfig)] = config;
        
        var jsonString = JsonSerializer.Serialize(config, MappingOptions.JsonSerializerOptions);
        var fileName = Path.Combine(configDir, config.GetType().Name + encryptedFileExtension);

        var encrypted = await _crypt.EncryptAsync(jsonString);
        
        await File.WriteAllTextAsync(fileName, encrypted);
    }

    public virtual async Task<TConfig?> ReadAsync<TConfig>() where TConfig : ConfigBase
    {
        var fileName = Path.Combine(configDir, typeof(TConfig).Name + encryptedFileExtension);
        if (!File.Exists(fileName))
            return null;
        
        var decrypted = await _crypt.DecryptAsync(await File.ReadAllTextAsync(fileName));
        var config = JsonSerializer.Deserialize<TConfig>(decrypted, MappingOptions.JsonSerializerOptions);
        
        if (!cachedConfigs.ContainsKey(config.GetType()))
            cachedConfigs.TryAdd(typeof(TConfig), config);

        return config;
    }
}