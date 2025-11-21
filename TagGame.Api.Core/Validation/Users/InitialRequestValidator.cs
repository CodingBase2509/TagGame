using FluentValidation;
using TagGame.Shared.DTOs.Auth;
using TagGame.Shared.Validation;

namespace TagGame.Api.Core.Validation.Users;

public class InitialRequestValidator : AbstractValidator<InitialRequestDto>
{
    public InitialRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .Custom((value, context) =>
            {
                if (!UserInitRules.TryValidateDeviceId(value, out var error))
                    context.AddFailure(error!);
            });

        RuleFor(x => x.DisplayName)
            .Custom((value, context) =>
            {
                if (!UserInitRules.TryValidateDisplayName(value, out var error))
                    context.AddFailure(error!);
            });

        RuleFor(x => x.AvatarColor)
            .Custom((value, context) =>
            {
                if (!UserInitRules.TryValidateAvatarColor(value, out var error))
                    context.AddFailure(error!);
            });
    }
}
