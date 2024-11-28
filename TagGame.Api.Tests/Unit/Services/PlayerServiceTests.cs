using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Tests.Unit.Services;

public class PlayerServiceTests : TestBase
{
    private readonly Mock<IDataAccess> _dataAccessMock;
    private readonly PlayerService _playerService;

    public PlayerServiceTests()
    {
        _dataAccessMock = new Mock<IDataAccess>();
        _playerService = new PlayerService(_dataAccessMock.Object);
    }

    #region CreatePlayerAsync Tests

    [Fact]
    public async Task CreatePlayerAsync_ShouldReturnPlayer_WhenUserExistsAndPlayerIsAdded()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .Create();

        _dataAccessMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync(user);

        _dataAccessMock.Setup(db => db.Players.AddAsync(It.IsAny<Player>()))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.CreatePlayerAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(userId);
        result?.AvatarColor.Should().Be(user.DefaultAvatarColor);
        result?.UserName.Should().Be(user.DefaultName);
    }

    [Fact]
    public async Task CreatePlayerAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _playerService.CreatePlayerAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePlayerAsync_ShouldReturnNull_WhenPlayerAddFails()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Create<User>();

        _dataAccessMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync(user);

        _dataAccessMock.Setup(db => db.Players.AddAsync(It.IsAny<Player>()))
            .ReturnsAsync(false);

        // Act
        var result = await _playerService.CreatePlayerAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeletePlayerAsync Tests

    [Fact]
    public async Task DeletePlayerAsync_ShouldReturnTrue_WhenPlayerIsDeletedSuccessfully()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var player = _fixture.Create<Player>();

        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, false))
            .ReturnsAsync(player);

        _dataAccessMock.Setup(db => db.Players.DeleteAsync(player))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.DeletePlayerAsync(playerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePlayerAsync_ShouldReturnFalse_WhenPlayerDoesNotExist()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, false))
            .ReturnsAsync((Player?)null);

        // Act
        var result = await _playerService.DeletePlayerAsync(playerId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetPlayerById Tests

    [Fact]
    public async Task GetPlayerById_ShouldReturnPlayer_WhenPlayerExists()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var player = _fixture.Build<Player>()
            .With(p => p.Id, playerId)
            .Create();

        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, false))
            .ReturnsAsync(player);

        // Act
        var result = await _playerService.GetPlayerById(playerId);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(playerId);
    }

    [Fact]
    public async Task GetPlayerById_ShouldReturnNull_WhenPlayerDoesNotExist()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, false))
            .ReturnsAsync((Player?)null);

        // Act
        var result = await _playerService.GetPlayerById(playerId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdatePlayerAsync Tests

    [Fact]
    public async Task UpdatePlayerAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var player = _fixture.Create<Player>();

        _dataAccessMock.Setup(db => db.Players.UpdateAsync(player))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.UpdatePlayerAsync(player);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePlayerAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        // Arrange
        var player = _fixture.Create<Player>();

        _dataAccessMock.Setup(db => db.Players.UpdateAsync(player))
            .ReturnsAsync(false);

        // Act
        var result = await _playerService.UpdatePlayerAsync(player);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddPlayerToRoomAsync Tests

    [Fact]
    public async Task AddPlayerToRoomAsync_ShouldReturnTrue_WhenPlayerAndRoomExist()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var roomId = _fixture.Create<Guid>();

        var player = _fixture.Create<Player>();
        var room = _fixture.Build<GameRoom>()
            .With(r => r.Id, roomId)
            .Create();

        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, false))
            .ReturnsAsync(player);

        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync(room);

        _dataAccessMock.Setup(db => db.Rooms.UpdateAsync(room))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.AddPlayerToRoomAsync(playerId, roomId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddPlayerToRoomAsync_ShouldReturnFalse_WhenPlayerOrRoomDoesNotExist()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var roomId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, false))
            .ReturnsAsync((Player?)null);

        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync((GameRoom?)null);

        // Act
        var result = await _playerService.AddPlayerToRoomAsync(playerId, roomId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RemovePlayerFromRoomAsync Tests

    [Fact]
    public async Task RemovePlayerFromRoomAsync_ShouldReturnTrue_WhenPlayerIsRemovedFromRoom()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var roomId = _fixture.Create<Guid>();

        var room = _fixture.Build<GameRoom>()
            .With(r => r.Id, roomId)
            .Create();

        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync(room);

        _dataAccessMock.Setup(db => db.Rooms.UpdateAsync(room))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.RemovePlayerFromRoomAsync(playerId, roomId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemovePlayerFromRoomAsync_ShouldReturnFalse_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync((GameRoom?)null);

        // Act
        var result = await _playerService.RemovePlayerFromRoomAsync(Guid.NewGuid(), roomId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}