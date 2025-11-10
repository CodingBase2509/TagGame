using System.Security.Cryptography;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Security;

public sealed class Crypto : ICrypto
{
    private const byte Version = 1;
    private const int NonceSize = 12;   // GCM-Standard
    private const int TagSize = 16;     // 128-bit Tag
    private const int KeySize = 32;     // 256-bit Key

    public Task<byte[]> Encrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> aad)
    {
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipherText = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plaintext, cipherText, tag, aad);

            var output = new byte[1 + NonceSize + cipherText.Length + TagSize];
            output[0] = Version;

            Buffer.BlockCopy(nonce, 0, output, 1, NonceSize);
            Buffer.BlockCopy(cipherText, 0, output, 1 + NonceSize, cipherText.Length);
            Buffer.BlockCopy(tag, 0, output, 1 + NonceSize + cipherText.Length, TagSize);

            return Task.FromResult(output);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(nonce.AsSpan());
            CryptographicOperations.ZeroMemory(cipherText.AsSpan());
            CryptographicOperations.ZeroMemory(tag.AsSpan());
        }
    }

    public Task<byte[]> Decrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> blob, ReadOnlySpan<byte> aad)
    {
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));
        if (blob.Length < 1 + NonceSize + TagSize)
            throw new ArgumentException("Ciphertext blob too short.", nameof(blob));

        var version = blob[0];
        if (version != Version)
            throw new CryptographicException($"Unsupported blob version {version}.");

        var nonce = blob.Slice(1, NonceSize);
        var ctLen = blob.Length - 1 - NonceSize - TagSize;
        if (ctLen < 0)
            throw new ArgumentException("Ciphertext blob invalid.", nameof(blob));

        var cipherText = blob.Slice(1 + NonceSize, ctLen);
        var tag = blob.Slice(1 + NonceSize + ctLen, TagSize);

        var plainText = new byte[ctLen];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipherText, tag, plainText, aad);
        return Task.FromResult(plainText);
    }
}
