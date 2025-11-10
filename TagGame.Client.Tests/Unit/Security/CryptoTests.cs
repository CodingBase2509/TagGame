using System.Security.Cryptography;
using System.Text;
using TagGame.Client.Core.Security;

namespace TagGame.Client.Tests.Unit.Security;

public class CryptoTests
{
    private static byte[] NewKey(int len = 32) => RandomNumberGenerator.GetBytes(len);
    private static byte[] Bytes(int len)
    {
        var b = new byte[len];
        for (int i = 0; i < len; i++) b[i] = (byte)(i % 251);
        return b;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(32)]
    [InlineData(1024)]
    public async Task Encrypt_then_Decrypt_roundtrips(int size)
    {
        var sut = new Crypto();
        var key = NewKey();
        var aad = Encoding.UTF8.GetBytes("tg.local/v1|test");
        var plain = Bytes(size);

        var blob = await sut.Encrypt(key, plain, aad);
        var clear = await sut.Decrypt(key, blob, aad);

        clear.Should().BeEquivalentTo(plain);
    }

    [Fact]
    public async Task Same_plaintext_produces_different_blobs_due_to_nonce()
    {
        var sut = new Crypto();
        var key = NewKey();
        var aad = Encoding.UTF8.GetBytes("tg.local/v1|file");
        var plain = Bytes(128);

        var blob1 = await sut.Encrypt(key, plain, aad);
        var blob2 = await sut.Encrypt(key, plain, aad);

        blob1.Should().NotBeEquivalentTo(blob2);
    }

    [Fact]
    public async Task Blob_contains_expected_structure_length()
    {
        // Expected: version(1) + nonce(12) + ciphertext(N) + tag(16)
        const int header = 1 + 12 + 16;
        var sut = new Crypto();
        var key = NewKey();
        var aad = Array.Empty<byte>();
        var plain = Bytes(77);

        var blob = await sut.Encrypt(key, plain, aad);

        blob.Length.Should().Be(header + plain.Length);
        blob[0].Should().Be(1); // version byte
    }

    [Fact]
    public async Task Decrypt_with_wrong_aad_throws()
    {
        var sut = new Crypto();
        var key = NewKey();
        var aad1 = Encoding.UTF8.GetBytes("tg.local/v1|a");
        var aad2 = Encoding.UTF8.GetBytes("tg.local/v1|b");
        var plain = Bytes(64);

        var blob = await sut.Encrypt(key, plain, aad1);
        var act = async () => await sut.Decrypt(key, blob, aad2);

        await act.Should().ThrowAsync<CryptographicException>();
    }

    [Fact]
    public async Task Decrypt_with_tampered_blob_throws()
    {
        var sut = new Crypto();
        var key = NewKey();
        var aad = Encoding.UTF8.GetBytes("tg.local/v1|tamper");
        var plain = Bytes(256);

        var blob = await sut.Encrypt(key, plain, aad);
        // Flip last byte (part of the tag region) to trigger GCM tag mismatch
        var tampered = (byte[])blob.Clone();
        tampered[^1] ^= 0xFF;

        var act = async () => await sut.Decrypt(key, tampered, aad);
        await act.Should().ThrowAsync<CryptographicException>();
    }

    [Fact]
    public async Task Encrypt_throws_on_invalid_key_length()
    {
        var sut = new Crypto();
        var badKey = NewKey(31);
        var act = async () => await sut.Encrypt(badKey, Bytes(1), []);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Decrypt_throws_on_invalid_key_length()
    {
        var sut = new Crypto();
        var key = NewKey();
        var blob = await sut.Encrypt(key, Bytes(8), []);

        var act = async () => await sut.Decrypt(NewKey(33), blob, []);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Decrypt_throws_on_short_blob()
    {
        var sut = new Crypto();
        var key = NewKey();
        var shortBlob = new byte[5];
        var act = async () => await sut.Decrypt(key, shortBlob, []);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Decrypt_throws_on_unsupported_version()
    {
        var sut = new Crypto();
        var key = NewKey();
        var blob = await sut.Encrypt(key, Bytes(10), []);
        blob[0] = 2; // unsupported version

        var act = async () => await sut.Decrypt(key, blob, []);
        await act.Should().ThrowAsync<CryptographicException>();
    }
}

