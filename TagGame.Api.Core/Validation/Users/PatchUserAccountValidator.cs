using FluentValidation;
using TagGame.Shared.DTOs.Users;
using TagGame.Shared.Validation;

namespace TagGame.Api.Core.Validation.Users;

public class PatchUserAccountValidator : AbstractValidator<PatchUserAccountDto>
{
    public PatchUserAccountValidator()
    {
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

        // Email: optional; if provided validate format and length
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .Must(s => string.IsNullOrWhiteSpace(s) || s.Trim() == s)
                .WithMessage("Email must not start or end with spaces.")
            .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Email must be at most 256 characters.")
            .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Email must be a valid email address.");

        // DeviceId: optional; if provided validate length and characters
        RuleFor(x => x.DeviceId)
            .Custom((value, context) =>
            {
                if (!UserInitRules.TryValidateDeviceId(value, out var error))
                    context.AddFailure(error!);
            });
    }
}
