using System.Security.Cryptography;
using System.Text.Json;
using TagGame.Shared.Constants;
using Color = System.Drawing.Color;

namespace TagGame.Client.Services;

public class ConfigHandler(Encryption crypt)
{
    private const string encryptedFileExtension = ".enc";
    private static string configDir = FileSystem.Current.AppDataDirectory;
    private readonly Dictionary<Type, ConfigBase> cachedConfigs = [];
    
    public async Task InitAsync()
    {
        // generate encryption key
        var ss = SecureStorage.Default;
        var aes = Aes.Create();
        await ss.SetAsync("crypt_key", JsonSerializer.Serialize(aes.Key, MappingOptions.JsonSerializerOptions));
        await ss.SetAsync("crypt_iv", JsonSerializer.Serialize(aes.IV, MappingOptions.JsonSerializerOptions));
        
        // generate default configs
        await WriteAsync(new ServerConfig());
        await WriteAsync(new UserConfig());
    }
    
    public async Task WriteAsync<TConfig>(TConfig config) where TConfig : ConfigBase
    {
        if (!cachedConfigs.ContainsKey(config.GetType()))
            cachedConfigs.TryAdd(typeof(TConfig), config);
        else
            cachedConfigs[typeof(TConfig)] = config;
        
        var jsonString = JsonSerializer.Serialize(config, MappingOptions.JsonSerializerOptions);
        var fileName = Path.Combine(configDir, config.GetType().Name + encryptedFileExtension);

        var encrypted = await crypt.EncryptAsync(jsonString);
        
        await File.WriteAllBytesAsync(fileName, encrypted);
    }

    public async Task<TConfig?> ReadAsync<TConfig>() where TConfig : ConfigBase
    {
        var fileName = Path.Combine(configDir, typeof(TConfig).Name + encryptedFileExtension);
        if (!File.Exists(fileName))
            return null;
        
        var decrypted = await crypt.DecryptAsync(await File.ReadAllBytesAsync(fileName));
        var config = JsonSerializer.Deserialize<TConfig>(decrypted, MappingOptions.JsonSerializerOptions);
        
        if (!cachedConfigs.ContainsKey(config.GetType()))
            cachedConfigs.TryAdd(typeof(TConfig), config);

        return config;
    }
}