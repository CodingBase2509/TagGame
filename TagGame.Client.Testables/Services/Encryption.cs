using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;
using TagGame.Shared.Constants;

namespace TagGame.Client.Services;

public class Encryption
{
    private static Aes? aes;
    public bool HasKeysLoaded { get; private set; }
    
    public async Task<byte[]> EncryptAsync(string text)
    {
        if (aes is null)
            await LoadKeyAsync();
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        await using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        await using var sw = new StreamWriter(cs);
        
        await sw.WriteAsync(text);
        return ms.ToArray();
    }

    public async Task<string> DecryptAsync(byte[] encrypted)
    {
        if (aes is null)
            await LoadKeyAsync();
        
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(encrypted);
        await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return await sr.ReadToEndAsync();
    }
    
    private async Task LoadKeyAsync()
    {
        if (aes is not null)
            return;

        aes = Aes.Create();
        
        var ss = SecureStorage.Default;
        
        var key = JsonSerializer.Deserialize<byte[]>(
            await ss.GetAsync("crypt_key") ?? string.Empty, MappingOptions.JsonSerializerOptions);
        var iv = JsonSerializer.Deserialize<byte[]>(
            await ss.GetAsync("crypt_iv") ?? string.Empty, MappingOptions.JsonSerializerOptions);

        if (key is not null && iv is not null)
        {   
            aes.Key = key;
            aes.IV = iv;
            HasKeysLoaded = true;
        }
    }
}