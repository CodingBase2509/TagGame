using FluentValidation;
using TagGame.Shared.DTOs.Settings;
using TagGame.Shared.Validation;

namespace TagGame.Api.Core.Validation.Rooms;

public sealed class PatchRoomSettingsRequestValidator : AbstractValidator<PatchRoomSettingsRequestDto>
{
    public PatchRoomSettingsRequestValidator()
    {
        RuleFor(x => x)
            .Custom((value, context) =>
            {
                if (!RoomSettingsRules.TryValidatePatchNotEmpty(value.HideTimeSec, value.HuntTimeSec, value.TagRadiusM, out var error))
                    context.AddFailure(error!);
            });

        RuleFor(x => x.HideTimeSec)
            .Custom((value, context) =>
            {
                if (!RoomSettingsRules.TryValidateHideTimeSec(value, out var error))
                    context.AddFailure(error!);
            });

        RuleFor(x => x.HuntTimeSec)
            .Custom((value, context) =>
            {
                if (!RoomSettingsRules.TryValidateHuntTimeSec(value, out var error))
                    context.AddFailure(error!);
            });

        RuleFor(x => x.TagRadiusM)
            .Custom((value, context) =>
            {
                if (!RoomSettingsRules.TryValidateTagRadiusM(value, out var error))
                    context.AddFailure(error!);
            });
    }
}
