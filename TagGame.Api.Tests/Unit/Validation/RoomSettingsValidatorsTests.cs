using TagGame.Api.Core.Validation.Rooms;
using TagGame.Shared.DTOs.Settings;

namespace TagGame.Api.Tests.Unit.Validation;

public sealed class RoomSettingsValidatorsTests
{
    [Fact]
    public void PatchRoomSettingsRequestValidator_accepts_valid_partial_patch()
    {
        var validator = new PatchRoomSettingsRequestValidator();
        var dto = new PatchRoomSettingsRequestDto { HideTimeSec = 120 };

        var res = validator.Validate(dto);

        res.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PatchRoomSettingsRequestValidator_rejects_empty_patch()
    {
        var validator = new PatchRoomSettingsRequestValidator();
        var dto = new PatchRoomSettingsRequestDto();

        var res = validator.Validate(dto);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.Rooms.Settings.Patch.Required");
    }

    [Fact]
    public void PatchRoomSettingsRequestValidator_rejects_out_of_range_values()
    {
        var validator = new PatchRoomSettingsRequestValidator();
        var dto = new PatchRoomSettingsRequestDto
        {
            HideTimeSec = 0,
            HuntTimeSec = 0,
            TagRadiusM = 0.0
        };

        var res = validator.Validate(dto);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.Rooms.Settings.HideTimeSec.Min");
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.Rooms.Settings.HuntTimeSec.Min");
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.Rooms.Settings.TagRadiusM.Min");
    }

    [Fact]
    public void PatchRoomSettingsRequestValidator_rejects_invalid_tag_radius()
    {
        var validator = new PatchRoomSettingsRequestValidator();
        var dto = new PatchRoomSettingsRequestDto { TagRadiusM = double.NaN };

        var res = validator.Validate(dto);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.ErrorMessage == "Errors.Validation.Rooms.Settings.TagRadiusM.Invalid");
    }
}

