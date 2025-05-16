using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Storage;
using TagGame.Shared.Constants;

namespace TagGame.Client.Services;

public class ConfigHandler (Encryption crypt, ISecureStorage secureStorage, IOptions<JsonSerializerOptions> jsonOptions, string configDir)
{
    private const string encryptedFileExtension = ".enc";
    private const string storageKey = "config_crypt";
    private readonly Dictionary<Type, ConfigBase> cachedConfigs = [];

    private readonly Encryption _crypt = crypt is not null ? crypt.WithStorageKey(storageKey) : crypt;

    public bool CanInteractWithFiles => _crypt.HasKeysLoaded;

    public ConfigHandler()
        : this(null, null, null, string.Empty)
    { }
    
    public async Task InitAsync()
    {
        // check if config key exists
        var key = await secureStorage.GetAsync(storageKey + "_key");
        if (key is not null)
            return;
        
        // generate encryption key
        var aes = Aes.Create();
        var jsonKey = JsonSerializer.Serialize(aes.Key, jsonOptions.Value);
        await secureStorage.SetAsync(storageKey + "_key", jsonKey);
        var jsonIv = JsonSerializer.Serialize(aes.IV, jsonOptions.Value);
        await secureStorage.SetAsync(storageKey + "_iv", jsonIv);
    }
    
    public virtual async Task WriteAsync<TConfig>(TConfig config) where TConfig : ConfigBase
    {
        if (!cachedConfigs.ContainsKey(config.GetType()))
            cachedConfigs.TryAdd(typeof(TConfig), config);
        else
            cachedConfigs[typeof(TConfig)] = config;
        
        var jsonString = JsonSerializer.Serialize(config, jsonOptions.Value);
        if (string.IsNullOrEmpty(jsonString))
            return;
        
        var fileName = Path.Combine(configDir, config.GetType().Name + encryptedFileExtension);

        var encrypted = await _crypt.EncryptAsync(jsonString);
        
        await File.WriteAllTextAsync(fileName, encrypted);
    }

    public virtual async Task<TConfig?> ReadAsync<TConfig>() where TConfig : ConfigBase
    {
        if (cachedConfigs.TryGetValue(typeof(TConfig), out var existingConfig))
        {
            return existingConfig as TConfig;
        }
        
        var fileName = Path.Combine(configDir, typeof(TConfig).Name + encryptedFileExtension);
        if (!File.Exists(fileName))
            return null;

        var file = await File.ReadAllTextAsync(fileName);
        var decrypted = await _crypt.DecryptAsync(file);
        if (string.IsNullOrEmpty(decrypted))
            return null;
        
        var config = JsonSerializer.Deserialize<TConfig>(decrypted, jsonOptions.Value);
        
        if (!cachedConfigs.ContainsKey(config.GetType()))
            cachedConfigs.TryAdd(typeof(TConfig), config);

        return config;
    }

    public virtual bool Exists<TConfig>() where TConfig : ConfigBase
    {
        cachedConfigs.TryGetValue(typeof(TConfig), out var existingConfig);
        
        var fileName = Path.Combine(configDir, typeof(TConfig).Name + encryptedFileExtension);
        return existingConfig is not null || File.Exists(fileName);
    }
    
    public virtual bool Delete<TConfig>() where TConfig : ConfigBase
    {
        if (cachedConfigs.TryGetValue(typeof(TConfig), out var _))
        {
            cachedConfigs.Remove(typeof(TConfig));
        }
        
        var fileName = Path.Combine(configDir, typeof(TConfig).Name + encryptedFileExtension);
        if (!File.Exists(fileName))
            return true;

        File.Delete(fileName);
        
        return !File.Exists(fileName);
    }
}