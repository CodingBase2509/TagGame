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
            error = "DeviceId is required.";
            return false;
        }

        var deviceId = raw.Trim();
        if (!ReferenceEquals(deviceId, raw))
        {
            error = "DeviceId must not start or end with spaces.";
            return false;
        }

        if (deviceId.Length > MaxDeviceIdLength)
        {
            error = "DeviceId must be at most 64 characters.";
            return false;
        }

        if (!DeviceIdRegex().IsMatch(deviceId))
        {
            error = "DeviceId may only contain letters, digits, '_' or '-'.";
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
            error = "Display name must not start or end with spaces.";
            return false;
        }

        if (name.Length < MinDisplayNameLength)
        {
            error = "Display name must be at least 2 characters.";
            return false;
        }

        if (name.Length > MaxDisplayNameLength)
        {
            error = "Display name must be at most 64 characters.";
            return false;
        }

        if (name.Any(char.IsControl))
        {
            error = "Display name contains invalid characters.";
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
            error = "Avatar color must be hex #RRGGBB or #AARRGGBB.";
            return false;
        }

        error = null;
        return true;
    }
}
