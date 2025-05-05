using Microsoft.AspNetCore.SignalR.Client;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;

namespace TagGame.Client.Tests.Unit.Clients;

public class LobbyClientTests : TestBase
{
    private readonly Mock<ConfigHandler> _configHandlerMock;
    private readonly LobbyClient _sut;  // SUT = System Under Test

    public LobbyClientTests()
    {
        _configHandlerMock = new Mock<ConfigHandler>();
        _sut = new LobbyClient(_configHandlerMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_ShouldNotThrow_WhenConfigIsValid()
    {
        // Arrange
        var serverConfig = _fixture.Build<ServerConfig>()
            .With(x => x.Host, "http://localhost")
            .With(x => x.Port, 5000)
            .Create();

        var userConfig = _fixture.Build<UserConfig>()
            .With(x => x.UserId, Guid.NewGuid())
            .Create();

        _configHandlerMock.Setup(x => x.ReadAsync<ServerConfig>())
            .ReturnsAsync(serverConfig);
        _configHandlerMock.Setup(x => x.ReadAsync<UserConfig>())
            .ReturnsAsync(userConfig);

        // Act
        Func<Task> act = async () => await _sut.InitializeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateGameSettingsAsync_ShouldNotInvoke_WhenConnectionIsNull()
    {
        // Arrange
        var settings = _fixture.Create<GameSettings>();

        // Act
        Func<Task> act = async () => await _sut.UpdateGameSettingsAsync(settings);

        // Assert
        await act.Should().NotThrowAsync();  // Method is safe to call even without connection
    }

    [Fact]
    public async Task StartGameAsync_ShouldNotInvoke_WhenConnectionIsNull()
    {
        // Act
        Func<Task> act = async () => await _sut.StartGameAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldNotThrow_WhenConnectionIsNull()
    {
        // Act
        Func<Task> act = async () => await _sut.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    // Erweiterte Tests mit gemocktem HubConnection

    [Fact]
    public async Task UpdateGameSettingsAsync_ShouldInvokeHub_WhenConnected()
    {
        // Arrange
        var settings = _fixture.Create<GameSettings>();
        var connectionMock = new Mock<IHubConnection>();
        typeof(LobbyClient).GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_sut, connectionMock.Object);

        connectionMock.Setup(c => c.State).Returns(HubConnectionState.Connected);

        // Act
        await _sut.UpdateGameSettingsAsync(settings);

        // Assert
        connectionMock.Verify(c => c.InvokeAsync(nameof(ApiRoutes.ILobbyHub.UpdateGameSettings), settings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartGameAsync_ShouldInvokeHub_WhenConnected()
    {
        // Arrange
        var connectionMock = new Mock<IHubConnection>();
        typeof(LobbyClient).GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_sut, connectionMock.Object);

        connectionMock.Setup(c => c.State).Returns(HubConnectionState.Connected);

        // Act
        await _sut.StartGameAsync();

        // Assert
        connectionMock.Verify(c => c.InvokeAsync(nameof(ApiRoutes.ILobbyHub.StartGame), It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}