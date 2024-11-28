using FluentValidation.TestHelper;
using TagGame.Api.Validation.GameRoom;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Tests.Unit.Validation;

public class CreateGameRoomValidatorTests : TestBase
{
    private readonly CreateGameRoomValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var request = _fixture.Build<CreateGameRoom.CreateGameRoomRequest>()
                              .With(x => x.UserId, Guid.Empty)
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("empty-id");
    }

    [Fact]
    public void ShouldNotHaveError_WhenUserIdIsValid()
    {
        // Arrange
        var request = _fixture.Build<CreateGameRoom.CreateGameRoomRequest>()
                              .With(x => x.UserId, Guid.NewGuid())
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void ShouldHaveError_WhenGameRoomNameIsEmpty()
    {
        // Arrange
        var request = _fixture.Build<CreateGameRoom.CreateGameRoomRequest>()
                              .With(x => x.GameRoomName, string.Empty)
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GameRoomName)
              .WithErrorMessage("empty-name");
    }

    [Fact]
    public void ShouldNotHaveError_WhenGameRoomNameIsValid()
    {
        // Arrange
        var request = _fixture.Build<CreateGameRoom.CreateGameRoomRequest>()
                              .With(x => x.GameRoomName, "ValidName")
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GameRoomName);
    }

    [Fact]
    public void ShouldHaveError_WhenGameRoomNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('A', MaxLengthOptions.GameNameLenght + 1);
        var request = _fixture.Build<CreateGameRoom.CreateGameRoomRequest>()
                              .With(x => x.GameRoomName, longName)
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GameRoomName)
              .WithErrorMessage("too-long-name");
    }

    [Fact]
    public void ShouldNotHaveError_WhenGameRoomNameIsAtMaxLength()
    {
        // Arrange
        var validName = new string('A', MaxLengthOptions.GameNameLenght);
        var request = _fixture.Build<CreateGameRoom.CreateGameRoomRequest>()
                              .With(x => x.GameRoomName, validName)
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GameRoomName);
    }
}