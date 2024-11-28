using System.Drawing;
using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Tests.Unit.Services;

public class UserServiceTests : TestBase
{
    private readonly Mock<IDataAccess> _dataServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _dataServiceMock = new Mock<IDataAccess>();
        _userService = new UserService(_dataServiceMock.Object);
    }

    #region AddUserAsync Tests

    [Fact]
    public async Task AddUserAsync_ShouldReturnUser_WhenUserIsAddedSuccessfully()
    {
        // Arrange
        var username = _fixture.Create<string>();
        var avatarColor = Color.FromArgb(255, 100, 150, 200);

        var user = new User
        {
            Id = Guid.NewGuid(),
            DefaultName = username,
            DefaultAvatarColor = avatarColor
        };

        _dataServiceMock.Setup(db => db.Users.AddAsync(It.Is<User>(u =>
            u.DefaultName == username && u.DefaultAvatarColor == avatarColor)))
            .ReturnsAsync(true);

        _dataServiceMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.AddUserAsync(username, avatarColor);

        // Assert
        result.Should().NotBeNull();
        result?.DefaultName.Should().Be(username);
        result?.DefaultAvatarColor.Should().Be(avatarColor);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnNull_WhenAddFails()
    {
        // Arrange
        var username = _fixture.Create<string>();
        var avatarColor = Color.FromArgb(255, 50, 50, 50);

        _dataServiceMock.Setup(db => db.Users.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.AddUserAsync(username, avatarColor);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnNull_WhenSaveChangesFails()
    {
        // Arrange
        var username = _fixture.Create<string>();
        var avatarColor = Color.FromArgb(255, 200, 200, 200);

        _dataServiceMock.Setup(db => db.Users.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        _dataServiceMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.AddUserAsync(username, avatarColor);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CheckIfUserExists Tests

    [Fact]
    public async Task CheckIfUserExists_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = new User { Id = userId };

        _dataServiceMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.CheckIfUserExists(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckIfUserExists_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        _dataServiceMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.CheckIfUserExists(userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserIsDeletedSuccessfully()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = new User { Id = userId };

        _dataServiceMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync(user);

        _dataServiceMock.Setup(db => db.Users.DeleteAsync(user))
            .ReturnsAsync(true);

        _dataServiceMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        _dataServiceMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalse_WhenDeleteFails()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = new User { Id = userId };

        _dataServiceMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync(user);

        _dataServiceMock.Setup(db => db.Users.DeleteAsync(user))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalse_WhenSaveChangesFails()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = new User { Id = userId };

        _dataServiceMock.Setup(db => db.Users.GetByIdAsync(userId, false))
            .ReturnsAsync(user);

        _dataServiceMock.Setup(db => db.Users.DeleteAsync(user))
            .ReturnsAsync(true);

        _dataServiceMock.Setup(db => db.SaveChangesAsync(default))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}