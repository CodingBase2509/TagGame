namespace TagGame.Client.Core.Services;

public interface ICrypto
{
    Task<byte[]> Encrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> aad);

    Task<byte[]> Decrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> blob, ReadOnlySpan<byte> aad);
}
