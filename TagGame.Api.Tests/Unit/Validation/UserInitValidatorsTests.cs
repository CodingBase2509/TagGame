using TagGame.Api.Core.Validation.Users;
using TagGame.Shared.DTOs.Auth;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Tests.Unit.Validation;

public class UserInitValidatorsTests
{
    [Fact]
    public void InitialRequestValidator_accepts_valid_payload()
    {
        var validator = new InitialRequestValidator();
        var dto = new InitialRequestDto { DeviceId = "Device_1", DisplayName = "Alex", AvatarColor = "#FFAABB" };

        var res = validator.Validate(dto);

        res.IsValid.Should().BeTrue();
    }

    [Fact]
    public void InitialRequestValidator_rejects_invalid_fields()
    {
        var validator = new InitialRequestValidator();
        var dto = new InitialRequestDto { DeviceId = " bad id ", DisplayName = " A ", AvatarColor = "#GGGGGG" };

        var res = validator.Validate(dto);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.DeviceId.NoEdgeSpaces" || e.ErrorMessage == "Errors.Validation.DeviceId.InvalidCharacters");
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.DisplayName.NoEdgeSpaces" || e.ErrorMessage == "Errors.Validation.DisplayName.MinLength");
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.AvatarColor.InvalidFormat");
    }

    [Fact]
    public void PatchUserAccountValidator_accepts_optional_fields_and_rejects_invalids()
    {
        var validator = new PatchUserAccountValidator();

        // Accepts empty optional fields
        var ok = validator.Validate(new PatchUserAccountDto { DisplayName = "Alex", AvatarColor = "#FFAA00", Email = null, DeviceId = "Device_1" });
        ok.IsValid.Should().BeTrue();

        // Rejects invalid email whitespace, length and format
        var badEmail = validator.Validate(new PatchUserAccountDto { DisplayName = "Alex", AvatarColor = "#FFAA00", Email = " test@example.com " });
        badEmail.IsValid.Should().BeFalse();
        badEmail.Errors.Should().Contain(e => e.ErrorMessage.Contains("must not start or end with spaces", StringComparison.OrdinalIgnoreCase));

        var badFormat = validator.Validate(new PatchUserAccountDto { DisplayName = "Alex", AvatarColor = "#FFAA00", Email = "invalid" });
        badFormat.IsValid.Should().BeFalse();
        badFormat.Errors.Should().Contain(e => e.ErrorMessage.Contains("valid email", StringComparison.OrdinalIgnoreCase));

        // Reject invalid device id
        var badDevice = validator.Validate(new PatchUserAccountDto { DisplayName = "Alex", AvatarColor = "#FFAA00", DeviceId = " bad id " });
        badDevice.IsValid.Should().BeFalse();
        badDevice.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.DeviceId.NoEdgeSpaces" || e.ErrorMessage == "Errors.Validation.DeviceId.InvalidCharacters");
    }
}
