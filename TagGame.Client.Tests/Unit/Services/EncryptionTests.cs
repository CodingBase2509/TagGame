using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Maui.Storage;
using TagGame.Client.Services;
using TagGame.Shared.Constants;

namespace TagGame.Client.Tests.Unit.Services;

public class EncryptionTests : TestBase
{
    private readonly Mock<ISecureStorage> _secureStorageMock;
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly string _storageKey = "crypt";

    public EncryptionTests()
    {
        _secureStorageMock = new Mock<ISecureStorage>();
        var aes = Aes.Create();
        _key = aes.Key;
        _iv = aes.IV;

        var keyJson = JsonSerializer.Serialize(_key, MappingOptions.JsonSerializerOptions);
        var ivJson = JsonSerializer.Serialize(_iv, MappingOptions.JsonSerializerOptions);

        _secureStorageMock.Setup(x => x.GetAsync("crypt_key")).ReturnsAsync(keyJson);
        _secureStorageMock.Setup(x => x.GetAsync("crypt_iv")).ReturnsAsync(ivJson);
    }

    private Encryption CreateEncryption() =>
        new Encryption(_secureStorageMock.Object).WithStorageKey(_storageKey);

    [Fact]
    public async Task EncryptAsync_ShouldReturnBase64String()
    {
        var encryption = CreateEncryption();
        var plainText = "TopSecretText";

        var encrypted = await encryption.EncryptAsync(plainText);

        encrypted.Should().NotBeNullOrWhiteSpace();
        Convert.FromBase64String(encrypted).Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DecryptAsync_ShouldReturnOriginalText()
    {
        var encryption = CreateEncryption();
        var plainText = "MyPassword123!";

        var encrypted = await encryption.EncryptAsync(plainText);
        var decrypted = await encryption.DecryptAsync(encrypted);

        decrypted.Should().Be(plainText);
    }

    [Fact]
    public async Task HasKeysLoaded_ShouldBeTrue_AfterEncrypt()
    {
        var encryption = CreateEncryption();

        await encryption.EncryptAsync("TestData");

        encryption.HasKeysLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task EncryptAsync_ShouldReturnEmpty_WhenKeyOrIvMissing()
    {
        var brokenStorageMock = new Mock<ISecureStorage>();
        brokenStorageMock.Setup(x => x.GetAsync("crypt_key")).ReturnsAsync((string?)null);
        brokenStorageMock.Setup(x => x.GetAsync("crypt_iv")).ReturnsAsync((string?)null);

        var encryption = new Encryption(brokenStorageMock.Object).WithStorageKey(_storageKey);

        var encrypted = await encryption.EncryptAsync("data");

        encrypted.Should().BeEmpty();
    }

    [Fact]
    public async Task EncryptAsync_ShouldReturnEmpty_WhenJsonIsInvalid()
    {
        var brokenStorageMock = new Mock<ISecureStorage>();
        brokenStorageMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync("not-json");

        var encryption = new Encryption(brokenStorageMock.Object).WithStorageKey(_storageKey);

        var encrypted = await encryption.EncryptAsync("data");

        encrypted.Should().BeEmpty();
    }

    [Fact]
    public async Task DecryptAsync_ShouldReturnEmpty_WhenInputIsInvalidBase64()
    {
        var encryption = CreateEncryption();
        await encryption.EncryptAsync("Init");

        var decrypted = await encryption.DecryptAsync("not-a-valid-base64");

        decrypted.Should().BeEmpty();
    }

    [Fact]
    public async Task EncryptAsync_ShouldReturnEmpty_WhenStorageKeyNotSet()
    {
        var encryption = new Encryption(_secureStorageMock.Object); // Ohne .WithStorageKey

        var encrypted = await encryption.EncryptAsync("data");

        encrypted.Should().BeEmpty();
    }

    [Fact]
    public async Task DecryptAsync_ShouldReturnEmpty_WhenStorageKeyNotSet()
    {
        var encryption = new Encryption(_secureStorageMock.Object); // Ohne .WithStorageKey

        var decrypted = await encryption.DecryptAsync("dummy");

        decrypted.Should().BeEmpty();
    }
}