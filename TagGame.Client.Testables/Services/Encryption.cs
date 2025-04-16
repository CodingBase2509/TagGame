using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Storage;
using TagGame.Shared.Constants;

[assembly: InternalsVisibleTo("TagGame.Client.Tests")]
namespace TagGame.Client.Services;

public class Encryption(ISecureStorage secureStorage)
{
    private Aes? aes;
    private string storageKey = string.Empty;
    public virtual bool HasKeysLoaded { get; private set; }

    public Encryption()
        : this(null)
    { }
    
    public virtual Encryption WithStorageKey(string storageKey)
    {
        this.storageKey = storageKey;
        if (aes is null) 
            return this;
        
        aes.Dispose();
        aes = null;
        return this;
    }
    
    public virtual async Task<string> EncryptAsync(string text)
    {
        if (string.IsNullOrEmpty(storageKey))
            return string.Empty;
        
        try
        {
            if (aes is null)
                await LoadKeyAsync();

            using var encryptor = aes!.CreateEncryptor(aes.Key, aes.IV);
            byte[] textBuffer = Encoding.UTF8.GetBytes(text);

            var encryptedBuffer = encryptor.TransformFinalBlock(textBuffer, 0, textBuffer.Length);
            return Convert.ToBase64String(encryptedBuffer);
        }
        catch (Exception _)
        {
            return string.Empty;
        }
    }

    public virtual async Task<string> DecryptAsync(string encrypted)
    {
        if (string.IsNullOrEmpty(storageKey))
            return string.Empty;
        
        try
        {
            if (aes is null)
                await LoadKeyAsync();

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var encryptedBuffer = Convert.FromBase64String(encrypted);

            var decryptedBuffer = decryptor.TransformFinalBlock(encryptedBuffer, 0, encryptedBuffer.Length);
            return Encoding.UTF8.GetString(decryptedBuffer);
        }
        catch (Exception _)
        {
            return string.Empty;
        }
    }
    
    private async Task LoadKeyAsync()
    {
        if (aes is not null)
            return;

        aes = Aes.Create();
        var jsonKey = await secureStorage.GetAsync(storageKey + "_key");
        var jsonIv = await secureStorage.GetAsync(storageKey + "_iv");

        if (jsonKey is null || jsonIv is null)
        {
            return;
        }
        
        var key = JsonSerializer.Deserialize<byte[]>(jsonKey, MappingOptions.JsonSerializerOptions);
        var iv = JsonSerializer.Deserialize<byte[]>(jsonIv, MappingOptions.JsonSerializerOptions);

        if (key is not null && iv is not null)
        {   
            aes.Key = key;
            aes.IV = iv;
            HasKeysLoaded = true;
        }
    }
}