using FluentValidation.TestHelper;
using TagGame.Api.Validation.GameRoom;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Tests.Unit.Validation;

public class GameRoomSettingsValidatorTests : TestBase
{
    private readonly GameRoomSettingsValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.Id, Guid.Empty)
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("empty-id");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.Id, Guid.NewGuid())
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveError_WhenRoomIdIsEmpty()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.RoomId, Guid.Empty)
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoomId)
              .WithErrorMessage("empty-id");
    }

    [Fact]
    public void ShouldNotHaveError_WhenRoomIdIsValid()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.RoomId, Guid.NewGuid())
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoomId);
    }

    [Fact]
    public void ShouldHaveError_WhenSeekerIdsContainsEmptyGuid()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.SeekerIds, new List<Guid> { Guid.NewGuid(), Guid.Empty })
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SeekerIds);
    }

    [Fact]
    public void ShouldNotHaveError_WhenSeekerIdsAreValid()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.SeekerIds, new List<Guid> { Guid.NewGuid(), Guid.NewGuid() })
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SeekerIds);
    }

    [Fact]
    public void ShouldHaveError_WhenHideTimeoutIsZeroOrNegative()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.HideTimeout, TimeSpan.Zero)
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HideTimeout);
    }

    [Fact]
    public void ShouldNotHaveError_WhenHideTimeoutIsPositive()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.HideTimeout, TimeSpan.FromSeconds(10))
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HideTimeout);
    }

    [Fact]
    public void ShouldHaveError_WhenPingIntervalIsZeroOrNegative_AndIsPingEnabled()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.IsPingEnabled, true)
                               .With(x => x.PingInterval, TimeSpan.Zero)
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PingInterval);
    }

    [Fact]
    public void ShouldNotHaveError_WhenPingIntervalIsPositive_AndIsPingEnabled()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.IsPingEnabled, true)
                               .With(x => x.PingInterval, TimeSpan.FromSeconds(30))
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PingInterval);
    }

    [Fact]
    public void ShouldNotHaveError_WhenPingIntervalIsNull_AndIsPingDisabled()
    {
        // Arrange
        var settings = _fixture.Build<GameSettings>()
                               .With(x => x.IsPingEnabled, false)
                               .With(x => x.PingInterval, (TimeSpan?)null)
                               .Create();

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PingInterval);
    }
}