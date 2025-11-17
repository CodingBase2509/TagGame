namespace TagGame.Client.Core.Options;

/// <summary>
/// Immutable snapshot of app preferences. A new instance is published on any change.
/// </summary>
/// <param name="ThemeMode">Current theme mode (system/light/dark).</param>
/// <param name="Language">Current UI language.</param>
/// <param name="NotificationsEnabled">Whether notifications are enabled.</param>
public record class AppPreferencesSnapshot(
    ThemeMode ThemeMode,
    Language Language,
    bool NotificationsEnabled,
    string DeviceId,
    Guid UserId);
