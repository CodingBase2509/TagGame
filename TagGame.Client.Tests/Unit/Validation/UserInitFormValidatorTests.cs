using TagGame.Client.Core.Validation;

namespace TagGame.Client.Tests.Unit.Validation;

public class UserInitFormValidatorTests
{
    [Fact]
    public void Validate_returns_valid_when_all_ok()
    {
        var res = UserInitFormValidator.Validate("Device_1", "Alex", "#FFAABB");
        res.IsValid.Should().BeTrue();
        res.DeviceIdError.Should().BeNull();
        res.DisplayNameError.Should().BeNull();
        res.AvatarColorError.Should().BeNull();
    }

    [Fact]
    public void Validate_deviceId_invalid_characters()
    {
        var res = UserInitFormValidator.Validate("bad id", "Alex", "#FFAABB");
        res.IsValid.Should().BeFalse();
        res.DeviceIdError.Should().Be("Errors.Validation.DeviceId.InvalidCharacters");
    }

    [Fact]
    public void Validate_displayname_edge_spaces_invalid()
    {
        var res = UserInitFormValidator.Validate("Device_1", " Alex ", "#FFAABB");
        res.IsValid.Should().BeFalse();
        res.DisplayNameError.Should().Be("Errors.Validation.DisplayName.NoEdgeSpaces");
    }

    [Fact]
    public void Validate_avatarColor_invalid_format()
    {
        var res = UserInitFormValidator.Validate("Device_1", "Alex", "#GHIJKL");
        res.IsValid.Should().BeFalse();
        res.AvatarColorError.Should().Be("Errors.Validation.AvatarColor.InvalidFormat");
    }
}

