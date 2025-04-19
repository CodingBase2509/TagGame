using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Storage;
using TagGame.Client.Common;
using TagGame.Client.Services;
using TagGame.Shared.Constants;

namespace TagGame.Client.Tests.Unit.Services;

public class ConfigHandlerTests : TestBase, IAsyncLifetime
{
    private readonly Mock<Encryption> _encryptionMock;
    private readonly Mock<ISecureStorage> _secureStorageMock;
    private readonly string _configDir;
    private ConfigHandler _configHandler;

    public ConfigHandlerTests()
    {
        _encryptionMock = new Mock<Encryption>();
        _secureStorageMock = new Mock<ISecureStorage>();
        _configDir = Path.Combine(Path.GetTempPath(), "config-tests");

        Directory.CreateDirectory(_configDir);
    }
    
    private class DummyConfig : ConfigBase
    {
        public string Username { get; set; } = "user123";
        public int RetryCount { get; set; } = 3;
    }
    
    public Task InitializeAsync()
    {
        _encryptionMock.Setup(e => e.WithStorageKey(It.IsAny<string>())).Returns(_encryptionMock.Object);
        var jsonOptions = new Mock<IOptions<JsonSerializerOptions>>();
        jsonOptions.Setup(x => x.Value)
            .Returns(() =>
            {
                MappingOptions.JsonSerializerOptions.Converters.Add(new MauiColorJsonConverter());
                return MappingOptions.JsonSerializerOptions;
            });
        _configHandler = new ConfigHandler(_encryptionMock.Object, _secureStorageMock.Object, jsonOptions.Object, _configDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    { 
        Directory.Delete(_configDir, true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task InitAsync_ShouldStoreKeyAndIV()
    {
        // Act
        await _configHandler.InitAsync();

        // Assert
        _secureStorageMock.Verify(x => x.SetAsync("config_crypt_key", It.IsAny<string>()), Times.Once);
        _secureStorageMock.Verify(x => x.SetAsync("config_crypt_iv", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task WriteAsync_ShouldEncryptAndWriteFile()
    {
        // Arrange
        var config = new DummyConfig();
        _encryptionMock.Setup(e => e.EncryptAsync(It.IsAny<string>())).ReturnsAsync("encrypted_content");

        var path = Path.Combine(_configDir, nameof(DummyConfig) + ".enc");
        if (File.Exists(path)) File.Delete(path);

        // Act
        await _configHandler.WriteAsync(config);

        // Assert
        File.Exists(path).Should().BeTrue();
        var content = await File.ReadAllTextAsync(path);
        content.Should().Be("encrypted_content");
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnDecryptedAndDeserializedObject()
    {
        // Arrange
        var expected = new DummyConfig { Username = "readUser", RetryCount = 5 };
        var json = JsonSerializer.Serialize(expected, MappingOptions.JsonSerializerOptions);
        var encrypted = "some-encrypted";

        _encryptionMock.Setup(e => e.DecryptAsync(It.IsAny<string>())).ReturnsAsync(json);

        var path = Path.Combine(_configDir, nameof(DummyConfig) + ".enc");
        await File.WriteAllTextAsync(path, encrypted);

        // Act
        var result = await _configHandler.ReadAsync<DummyConfig>();

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("readUser");
        result.RetryCount.Should().Be(5);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnNull_WhenFileNotExists()
    {
        var path = Path.Combine(_configDir, nameof(DummyConfig) + ".enc");
        if (File.Exists(path)) File.Delete(path);

        var result = await _configHandler.ReadAsync<DummyConfig>();
        result.Should().BeNull();
    }

    [Fact]
    public void CanInteractWithFiles_ShouldReflectEncryptionState()
    {
        // Arrange
        _encryptionMock.Setup(e => e.HasKeysLoaded).Returns(true);

        // Act + Assert
        _configHandler.CanInteractWithFiles.Should().BeTrue();
    }
}