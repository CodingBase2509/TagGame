namespace TagGame.Shared.Validation;

/// <summary>
/// Shared validation rules for room creation and join flows.
/// Keeps error codes stable for localization on client and server.
/// </summary>
public static partial class RoomRules
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 64;
    private const int MinAccessCodeLength = 8;
    private const int MaxAccessCodeLength = 8;

    /// <summary>
    /// Validates a room name: required, trimmed, length bounds, no control chars.
    /// </summary>
    public static bool TryValidateName(string? raw, out string? error)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Errors.Validation.Rooms.Name.Required";
            return false;
        }

        var name = raw.Trim();
        if (!ReferenceEquals(name, raw))
        {
            error = "Errors.Validation.Rooms.Name.NoEdgeSpaces";
            return false;
        }

        if (name.Length < MinNameLength)
        {
            error = "Errors.Validation.Rooms.Name.MinLength";
            return false;
        }

        if (name.Length > MaxNameLength)
        {
            error = "Errors.Validation.Rooms.Name.MaxLength";
            return false;
        }

        if (name.Any(char.IsControl))
        {
            error = "Errors.Validation.Rooms.Name.InvalidCharacters";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Validates a room access code: required, trimmed, length bounds, alphanumeric.
    /// </summary>
    public static bool TryValidateAccessCode(string? raw, out string? error)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Errors.Validation.Rooms.AccessCode.Required";
            return false;
        }

        var code = raw.Trim();
        if (!ReferenceEquals(code, raw))
        {
            error = "Errors.Validation.Rooms.AccessCode.NoEdgeSpaces";
            return false;
        }

        if (code.Length < MinAccessCodeLength)
        {
            error = "Errors.Validation.Rooms.AccessCode.MinLength";
            return false;
        }

        if (code.Length > MaxAccessCodeLength)
        {
            error = "Errors.Validation.Rooms.AccessCode.MaxLength";
            return false;
        }

        if (!code.All(char.IsLetterOrDigit))
        {
            error = "Errors.Validation.Rooms.AccessCode.InvalidCharacters";
            return false;
        }

        error = null;
        return true;
    }
}
