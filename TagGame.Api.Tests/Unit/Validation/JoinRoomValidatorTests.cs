using FluentValidation.TestHelper;
using TagGame.Api.Validation.GameRoom;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Games;

namespace TagGameApi.Tests.Unit.Validation;

public class JoinRoomValidatorTests : TestBase
{
    private readonly JoinRoomValidator _validator = new();

    [Fact]
    public void ShouldNotHaveError_WhenAccessCodeIsValid()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
            .With(x => x.AccessCode, "Valid123")
            .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AccessCode);
    }
    
    [Fact]
    public void ShouldHaveError_WhenAccessCodeIsEmpty()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
                              .With(x => x.AccessCode, string.Empty) // Leerer AccessCode
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessCode)
              .WithErrorMessage("empty-accesscode");
    }

    [Fact]
    public void ShouldHaveError_WhenAccessCodeIsTooLong()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
                              .With(x => x.AccessCode, new string('A', MaxLengthOptions.AccessCodeLenght + 1)) // Überschreitet MaxLänge
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessCode)
              .WithErrorMessage("too-long-accesscode");
    }

    [Fact]
    public void ShouldNotHaveError_WhenGameNameIsValid()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
            .With(x => x.GameName, "ValidGameName") // Gültiger Name
            .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GameName);
    }
    
    [Fact]
    public void ShouldHaveError_WhenGameNameIsEmpty()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
                              .With(x => x.GameName, string.Empty) // Leerer Name
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GameName)
              .WithErrorMessage("empty-name");
    }

    [Fact]
    public void ShouldHaveError_WhenGameNameIsTooLong()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
                              .With(x => x.GameName, new string('B', MaxLengthOptions.GameNameLenght + 1)) // Überschreitet MaxLänge
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GameName)
              .WithErrorMessage("too-long-name");
    }

    [Fact]
    public void ShouldNotHaveError_WhenUserIdIsValid()
    {
        // Arrange
        var request = _fixture.Build<JoinGameRoom.JoinGameRoomRequest>()
                              .With(x => x.UserId, Guid.NewGuid()) // Gültiger UserId
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}