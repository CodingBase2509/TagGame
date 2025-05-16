using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;

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
        result?.UserId.Should().Be(userId);
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

    #region GetPlayerByUserId

    [Fact]
    public async Task GetPlayerByUserId_ShouldReturnPlayer_WhenPlayerExists()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var player = _fixture.Build<Player>()
            .With(p => p.UserId, userId)
            .Create();

        var players = new List<Player> { player }.AsQueryable();
        _dataAccessMock.Setup(db => db.Players.Where(It.IsAny<Func<Player, bool>>(), It.IsAny<bool>()))
            .Returns(players);

        // Act
        var result = await _playerService.GetPlayerByUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result?.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetPlayerByUserId_ShouldReturnNull_WhenPlayerDoesNotExist()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var players = new List<Player>().AsQueryable();
        
        _dataAccessMock.Setup(db => db.Players.Where(It.IsAny<Func<Player, bool>>(), It.IsAny<bool>()))
            .Returns(players);

        // Act
        var result = await _playerService.GetPlayerByUserId(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreatePlayerLeftGameAsync Tests

    [Fact]
    public async Task CreatePlayerLeftGameAsync_ShouldReturnPlayerLeftGameInfo_WhenSuccessful()
    {
        // Arrange
        var connectionId = "testConnectionId";
        var player = _fixture.Create<Player>();

        _dataAccessMock.Setup(db => db.Players.Where(It.IsAny<Func<Player, bool>>(), It.IsAny<bool>()))
            .Returns(new List<Player> { player }.AsQueryable());

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.AddAsync(It.IsAny<PlayerLeftGameInfo>()))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.CreatePlayerLeftGameAsync(connectionId);

        // Assert
        result.Should().NotBeNull();
        result!.Player.Should().Be(player);
        result.DisconnectType.Should().Be(PlayerDisconnectType.LeftGame);
    }

    [Fact]
    public async Task CreatePlayerLeftGameAsync_ShouldReturnNull_WhenPlayerNotFound()
    {
        // Arrange
        var connectionId = "testConnectionId";

        _dataAccessMock.Setup(db => db.Players.Where(It.IsAny<Func<Player, bool>>(), It.IsAny<bool>()))
            .Returns(new List<Player>().AsQueryable());

        // Act
        var result = await _playerService.CreatePlayerLeftGameAsync(connectionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePlayerLeftGameAsync_ShouldReturnNull_WhenAddingFails()
    {
        // Arrange
        var connectionId = "testConnectionId";
        var player = _fixture.Create<Player>();

        _dataAccessMock.Setup(db => db.Players.Where(It.IsAny<Func<Player, bool>>(), It.IsAny<bool>()))
            .Returns(new List<Player> { player }.AsQueryable());

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.AddAsync(It.IsAny<PlayerLeftGameInfo>()))
            .ReturnsAsync(false);

        // Act
        var result = await _playerService.CreatePlayerLeftGameAsync(connectionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePlayerLeftGameAsync_ShouldReturnNull_WhenSaveChangesFails()
    {
        // Arrange
        var connectionId = "testConnectionId";
        var player = _fixture.Create<Player>();

        _dataAccessMock.Setup(db => db.Players.Where(It.IsAny<Func<Player, bool>>(), It.IsAny<bool>()))
            .Returns(new List<Player> { player }.AsQueryable());

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.AddAsync(It.IsAny<PlayerLeftGameInfo>()))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(false);

        // Act
        var result = await _playerService.CreatePlayerLeftGameAsync(connectionId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPlayerLeftGame Tests

    [Fact]
    public async Task GetPlayerLeftGame_ShouldReturnPlayerLeftGameInfo_WhenPlayerExists()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var player = _fixture.Build<Player>()
            .With(p => p.Id, playerId)
            .Create();

        var expectedLeftInfo = _fixture.Build<PlayerLeftGameInfo>()
            .With(i => i.Player, player)
            .Create();

        _dataAccessMock.Setup(db => db.PlayerLeftInfo)
            .Returns(_dataAccessMock.Object.PlayerLeftInfo);

        _dataAccessMock.Setup(db =>
                db.PlayerLeftInfo.Where(It.IsAny<Func<PlayerLeftGameInfo, bool>>(), It.IsAny<bool>()))
            .Returns([expectedLeftInfo]);

        // Act
        var result = await _playerService.GetPlayerLeftGame(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedLeftInfo);
        result!.Player.Id.Should().Be(playerId);
    }

    [Fact]
    public async Task GetPlayerLeftGame_ShouldReturnNull_WhenPlayerDoesNotExist()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.PlayerLeftInfo)
            .Returns(_dataAccessMock.Object.PlayerLeftInfo);

        _dataAccessMock.Setup(db =>
                db.PlayerLeftInfo.Where(It.IsAny<Func<PlayerLeftGameInfo, bool>>(), It.IsAny<bool>()))
            .Returns([]);
        
        // Act
        var result = await _playerService.GetPlayerLeftGame(playerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlayerLeftGame_ShouldReturnNull_WhenPlayerLeftInfoIsNull()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.PlayerLeftInfo)
            .Returns(_dataAccessMock.Object.PlayerLeftInfo);

        _dataAccessMock.Setup(db =>
                db.PlayerLeftInfo.Where(It.IsAny<Func<PlayerLeftGameInfo, bool>>(), It.IsAny<bool>()))
            .Returns([]);

        // Act
        var result = await _playerService.GetPlayerLeftGame(playerId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeletePlayerLeftGameAsync Tests

    [Fact]
    public async Task DeletePlayerLeftGameAsync_ShouldReturnTrue_WhenPlayerLeftGameExists()
    {
        // Arrange
        var playerLeftId = _fixture.Create<Guid>();
        var playerLeftInfo = _fixture.Create<PlayerLeftGameInfo>();

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.GetByIdAsync(playerLeftId, false))
            .ReturnsAsync(playerLeftInfo);

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.DeleteAsync(playerLeftInfo))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.DeletePlayerLeftGameAsync(playerLeftId);

        // Assert
        result.Should().BeTrue();
        _dataAccessMock.Verify(db => db.PlayerLeftInfo.DeleteAsync(playerLeftInfo), Times.Once);
    }

    [Fact]
    public async Task DeletePlayerLeftGameAsync_ShouldReturnFalse_WhenPlayerLeftGameDoesNotExist()
    {
        // Arrange
        var playerLeftId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.GetByIdAsync(playerLeftId, false))
            .ReturnsAsync((PlayerLeftGameInfo?)null);

        // Act
        var result = await _playerService.DeletePlayerLeftGameAsync(playerLeftId);

        // Assert
        result.Should().BeFalse();
        _dataAccessMock.Verify(db => db.PlayerLeftInfo.DeleteAsync(It.IsAny<PlayerLeftGameInfo>()), Times.Never);
    }

    [Fact]
    public async Task DeletePlayerLeftGameAsync_ShouldReturnFalse_WhenDeletionFails()
    {
        // Arrange
        var playerLeftId = _fixture.Create<Guid>();
        var playerLeftInfo = _fixture.Create<PlayerLeftGameInfo>();

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.GetByIdAsync(playerLeftId, false))
            .ReturnsAsync(playerLeftInfo);

        _dataAccessMock.Setup(db => db.PlayerLeftInfo.DeleteAsync(playerLeftInfo))
            .ReturnsAsync(false);

        // Act
        var result = await _playerService.DeletePlayerLeftGameAsync(playerLeftId);

        // Assert
        result.Should().BeFalse();
        _dataAccessMock.Verify(db => db.PlayerLeftInfo.DeleteAsync(playerLeftInfo), Times.Once);
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

        var player = _fixture.Build<Player>()
            .With(p => p.Id, playerId)
            .Create();
        var room = _fixture.Build<GameRoom>()
            .With(r => r.Id, roomId)
            .Create();
        
        _dataAccessMock.Setup(db => db.Players.GetByIdAsync(playerId, It.IsAny<bool>()))
            .ReturnsAsync(player);

        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync(room);

        _dataAccessMock.Setup(db => db.Rooms.UpdateAsync(room))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _playerService.AddPlayerToRoomAsync(roomId, playerId);

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