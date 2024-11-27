using System.Drawing;
using FluentValidation.TestHelper;
using TagGame.Api.Validation.User;
using TagGame.Shared.DTOs.Users;

namespace TagGameApi.Tests.Unit.Validation;

public class CreateUserValidatorTests : TestBase
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var request = _fixture.Build<CreateUser.CreateUserRequest>()
                              .With(x => x.Name, string.Empty)
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("empty-name");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // Arrange
        var request = _fixture.Build<CreateUser.CreateUserRequest>()
                              .With(x => x.Name, "ValidName")
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenAvatarColorIsInvalid()
    {
        // Arrange
        Action invalidColor = () => Color.FromArgb(300, 255, 255, 255); // Ungültiges A

        // Act & Assert
        invalidColor.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ShouldNotHaveError_WhenAvatarColorIsValid()
    {
        // Arrange
        var request = _fixture.Build<CreateUser.CreateUserRequest>()
                              .With(x => x.AvatarColor, Color.FromArgb(255, 255, 128, 64))
                              .Create();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarColor);
    }

    [Theory]
    [InlineData(-1, 255, 255, 255)] // Ungültiges R
    [InlineData(255, -1, 255, 255)] // Ungültiges G
    [InlineData(255, 255, -1, 255)] // Ungültiges B
    [InlineData(255, 255, 255, -1)] // Ungültiges A
    [InlineData(256, 255, 255, 255)] // Ungültiges R
    [InlineData(255, 256, 255, 255)] // Ungültiges G
    [InlineData(255, 255, 256, 255)] // Ungültiges B
    [InlineData(255, 255, 255, 256)] // Ungültiges A
    public void ShouldHaveError_ForInvalidColorChannels(int r, int g, int b, int a)
    {
        // Arrange
        Action createInvalidColor = () => Color.FromArgb(a, r, g, b);

        // Act & Assert
        createInvalidColor.Should().Throw<ArgumentException>();
    }
}