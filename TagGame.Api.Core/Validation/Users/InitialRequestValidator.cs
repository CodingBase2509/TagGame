using System.Text.RegularExpressions;
using FluentValidation;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Api.Core.Validation.Users;

public partial class InitialRequestValidator : AbstractValidator<InitialRequestDto>
{
    private static readonly Regex HexColor = HexColorRegex();

    public InitialRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("DeviceId is required.")
            .Must(s => s!.Trim() == s)
                .WithMessage("DeviceId must not start or end with spaces.")
            .MaximumLength(64)
                .WithMessage("DeviceId must be at most 64 characters.")
            .Matches("^[A-Za-z0-9_-]+$")
                .WithMessage("DeviceId may only contain letters, digits, '_' or '-'.");

        RuleFor(x => x.DisplayName)
            .Cascade(CascadeMode.Stop)
            .Must(s => string.IsNullOrWhiteSpace(s) || s.Trim().Length >= 2)
                .WithMessage("Display name must be at least 2 characters.")
            .Must(s => string.IsNullOrWhiteSpace(s) || s.Trim().Length <= 64)
                .WithMessage("Display name must be at most 64 characters.")
            .Must(s => string.IsNullOrWhiteSpace(s) || s.Trim() == s)
                .WithMessage("Display name must not start or end with spaces.")
            .Must(s => string.IsNullOrWhiteSpace(s) || !ContainsControlCharacters(s))
                .WithMessage("Display name contains invalid characters.");

        RuleFor(x => x.AvatarColor)
            .Cascade(CascadeMode.Stop)
            .Must(s => string.IsNullOrWhiteSpace(s) || HexColor.IsMatch(s))
                .WithMessage("Avatar color must be hex #RRGGBB or #AARRGGBB.");
    }

    private static bool ContainsControlCharacters(string s) => s.Any(char.IsControl);

    [GeneratedRegex("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();
}
