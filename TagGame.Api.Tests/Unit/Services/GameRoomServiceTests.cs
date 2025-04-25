using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Tests.Unit.Services;

public class GameRoomServiceTests : TestBase
{
    private readonly Mock<IDataAccess> _dataAccessMock;
    private readonly GameRoomService _gameRoomService;

    public GameRoomServiceTests()
    {
        _dataAccessMock = new Mock<IDataAccess>();
        _gameRoomService = new GameRoomService(_dataAccessMock.Object);
    }

    #region GetRoomAsync Tests

    [Fact]
    public async Task GetRoomAsync_ShouldReturnRoom_WhenRoomExists()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();
        var room = _fixture.Build<GameRoom>().With(r => r.Id, roomId).Create();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync(room);

        // Act
        var result = await _gameRoomService.GetRoomAsync(roomId);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(roomId);
    }

    [Fact]
    public async Task GetRoomAsync_ShouldReturnNull_WhenRoomIdIsEmpty()
    {
        // Act
        var result = await _gameRoomService.GetRoomAsync(Guid.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomAsync_ShouldReturnNull_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync((GameRoom?)null);

        // Act
        var result = await _gameRoomService.GetRoomAsync(roomId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomAsync_ShouldReturnRoom_WhenNameAndAccessCodeMatch()
    {
        // Arrange
        var roomName = _fixture.Create<string>();
        var accessCode = _fixture.Create<string>();
        var room = _fixture.Build<GameRoom>()
            .With(r => r.Name, roomName)
            .With(r => r.AccessCode, accessCode)
            .Create();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.Where(It.IsAny<Func<GameRoom, bool>>(), It.IsAny<bool>()))
            .Returns(new[] { room }.AsQueryable());

        // Act
        var result = await _gameRoomService.GetRoomAsync(roomName, accessCode);

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be(roomName);
        result?.AccessCode.Should().Be(accessCode);
    }

    [Fact]
    public async Task GetRoomAsync_ShouldReturnNull_WhenNameOrAccessCodeIsNullOrEmpty()
    {
        // Act
        var result1 = await _gameRoomService.GetRoomAsync(string.Empty, "code");
        var result2 = await _gameRoomService.GetRoomAsync("name", string.Empty);

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    #endregion

    #region GetRoomFromPlayerAsync Tests
    
    [Fact]
    public async Task GetRoomFromPlayerAsync_ShouldReturnRoom_WhenPlayerIsInRoom()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();
        var player = _fixture.Build<Player>().With(p => p.Id, playerId).Create();
        var room = _fixture.Build<GameRoom>()
            .With(r => r.Players, new List<Player> { player })
            .Create();
        
        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Players))
            .Returns(_dataAccessMock.Object.Rooms);
        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        _dataAccessMock.Setup(db => db.Rooms.Where(It.IsAny<Func<GameRoom, bool>>(), It.IsAny<bool>()))
            .Returns(new[] { room }.AsQueryable());

        // Act
        var result = await _gameRoomService.GetRoomFromPlayerAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result?.Players.Should().ContainSingle(p => p.Id == playerId);
    }

    [Fact]
    public async Task GetRoomFromPlayerAsync_ShouldReturnNull_WhenPlayerIsNotInAnyRoom()
    {
        // Arrange
        var playerId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Players))
            .Returns(_dataAccessMock.Object.Rooms);
        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        _dataAccessMock.Setup(db => db.Rooms.Where(It.IsAny<Func<GameRoom, bool>>(), It.IsAny<bool>()))
            .Returns(new List<GameRoom>().AsQueryable());

        // Act
        var result = await _gameRoomService.GetRoomFromPlayerAsync(playerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomFromPlayerAsync_ShouldReturnNull_WhenPlayerIdIsEmpty()
    {
        // Act
        var result = await _gameRoomService.GetRoomFromPlayerAsync(Guid.Empty);

        // Assert
        result.Should().BeNull();
    }
    
    #endregion
    
    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldReturnRoom_WhenCreationIsSuccessful()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var roomName = _fixture.Create<string>();

        _dataAccessMock.Setup(db => db.Settings.AddAsync(It.IsAny<GameSettings>()))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.Rooms.AddAsync(It.IsAny<GameRoom>()))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _gameRoomService.CreateAsync(userId, roomName);

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be(roomName);
        result?.CreatorId.Should().Be(userId);
        result?.Settings.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCreationFails()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var roomName = _fixture.Create<string>();

        _dataAccessMock.Setup(db => db.Settings.AddAsync(It.IsAny<GameSettings>()))
            .ReturnsAsync(false);

        _dataAccessMock.Setup(db => db.Rooms.AddAsync(It.IsAny<GameRoom>()))
            .ReturnsAsync(false);

        // Act
        var result = await _gameRoomService.CreateAsync(userId, roomName);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteRoomAsync Tests

    [Fact]
    public async Task DeleteRoomAsync_ShouldReturnTrue_WhenRoomIsDeletedSuccessfully()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();
        var room = _fixture.Create<GameRoom>();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync(room);

        _dataAccessMock.Setup(db => db.Rooms.DeleteAsync(room))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _gameRoomService.DeleteRoomAsync(roomId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRoomAsync_ShouldReturnFalse_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync((GameRoom?)null);

        // Act
        var result = await _gameRoomService.DeleteRoomAsync(roomId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateSettingsAsync Tests

    [Fact]
    public async Task UpdateSettingsAsync_ShouldReturnTrue_WhenSettingsAreUpdatedSuccessfully()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();
        var settings = _fixture.Create<GameSettings>();
        var room = _fixture.Build<GameRoom>()
            .With(r => r.Id, roomId)
            .Create();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync(room);

        _dataAccessMock.Setup(db => db.Settings.UpdateAsync(settings))
            .ReturnsAsync(true);

        _dataAccessMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _gameRoomService.UpdateSettingsAsync(roomId, settings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSettingsAsync_ShouldReturnFalse_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = _fixture.Create<Guid>();
        var settings = _fixture.Create<GameSettings>();

        _dataAccessMock.Setup(db => db.Rooms.Include(r => r.Settings))
            .Returns(_dataAccessMock.Object.Rooms);
        
        _dataAccessMock.Setup(db => db.Rooms.GetByIdAsync(roomId, It.IsAny<bool>()))
            .ReturnsAsync((GameRoom?)null);

        // Act
        var result = await _gameRoomService.UpdateSettingsAsync(roomId, settings);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}