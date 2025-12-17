namespace TagGame.Shared.Validation;

/// <summary>
/// Shared validation rules for room settings.
/// Keeps error codes stable for localization on client and server.
/// </summary>
public static class RoomSettingsRules
{
    // Keep these ranges intentionally conservative; adjust once gameplay constraints are finalized.
    private const int MinHideTimeSec = 5;
    private const int MaxHideTimeSec = 600;

    private const int MinHuntTimeSec = 30;
    private const int MaxHuntTimeSec = 7200;

    private const double MinTagRadiusM = 0.5;
    private const double MaxTagRadiusM = 50.0;

    /// <summary>
    /// Validates a PATCH payload is not empty (at least one value must be provided).
    /// </summary>
    public static bool TryValidatePatchNotEmpty(int? hideTimeSec, int? huntTimeSec, double? tagRadiusM, out string? error)
    {
        if (hideTimeSec is null && huntTimeSec is null && tagRadiusM is null)
        {
            error = "Errors.Validation.Rooms.Settings.Patch.Required";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateHideTimeSec(int? value, out string? error)
    {
        switch (value)
        {
            case null:
                error = null;
                return true;
            case < MinHideTimeSec:
                error = "Errors.Validation.Rooms.Settings.HideTimeSec.Min";
                return false;
            case > MaxHideTimeSec:
                error = "Errors.Validation.Rooms.Settings.HideTimeSec.Max";
                return false;
            default:
                error = null;
                return true;
        }
    }

    public static bool TryValidateHuntTimeSec(int? value, out string? error)
    {
        switch (value)
        {
            case null:
                error = null;
                return true;
            case < MinHuntTimeSec:
                error = "Errors.Validation.Rooms.Settings.HuntTimeSec.Min";
                return false;
            case > MaxHuntTimeSec:
                error = "Errors.Validation.Rooms.Settings.HuntTimeSec.Max";
                return false;
            default:
                error = null;
                return true;
        }
    }

    public static bool TryValidateTagRadiusM(double? value, out string? error)
    {
        if (value is null)
        {
            error = null;
            return true;
        }

        if (double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            error = "Errors.Validation.Rooms.Settings.TagRadiusM.Invalid";
            return false;
        }

        switch (value.Value)
        {
            case < MinTagRadiusM:
                error = "Errors.Validation.Rooms.Settings.TagRadiusM.Min";
                return false;
            case > MaxTagRadiusM:
                error = "Errors.Validation.Rooms.Settings.TagRadiusM.Max";
                return false;
            default:
                error = null;
                return true;
        }
    }
}
