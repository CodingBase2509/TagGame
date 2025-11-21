using System.Text.RegularExpressions;

namespace TagGame.Shared.Validation;

/// <summary>
/// Shared validation rules for user init/profile fields (device id, display name, avatar color).
/// </summary>
public static partial class UserInitRules
{
    private const int MinDisplayNameLength = 2;
    private const int MaxDisplayNameLength = 64;
    private const int MaxDeviceIdLength = 64;

    [GeneratedRegex("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();

    [GeneratedRegex("^[A-Za-z0-9_-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex DeviceIdRegex();

    public static bool TryValidateDeviceId(string? raw, out string? error)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Errors.Validation.DeviceId.Required";
            return false;
        }

        var deviceId = raw.Trim();
        if (!ReferenceEquals(deviceId, raw))
        {
            error = "Errors.Validation.DeviceId.NoEdgeSpaces";
            return false;
        }

        if (deviceId.Length > MaxDeviceIdLength)
        {
            error = "Errors.Validation.DeviceId.MaxLength";
            return false;
        }

        if (!DeviceIdRegex().IsMatch(deviceId))
        {
            error = "Errors.Validation.DeviceId.InvalidCharacters";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateDisplayName(string? raw, out string? error)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = null;
            return true; // optional
        }

        var name = raw.Trim();
        if (!ReferenceEquals(name, raw))
        {
            error = "Errors.Validation.DisplayName.NoEdgeSpaces";
            return false;
        }

        if (name.Length < MinDisplayNameLength)
        {
            error = "Errors.Validation.DisplayName.MinLength";
            return false;
        }

        if (name.Length > MaxDisplayNameLength)
        {
            error = "Errors.Validation.DisplayName.MaxLength";
            return false;
        }

        if (name.Any(char.IsControl))
        {
            error = "Errors.Validation.DisplayName.InvalidCharacters";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateAvatarColor(string? raw, out string? error)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = null;
            return true; // optional
        }

        if (!HexColorRegex().IsMatch(raw))
        {
            error = "Errors.Validation.AvatarColor.InvalidFormat";
            return false;
        }

        error = null;
        return true;
    }
}
