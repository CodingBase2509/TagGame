using TagGame.Client.Core.Options;

namespace TagGame.Client.Core.Services;

/// <summary>
/// App-wide preferences (theme, language, notifications) with change events and persistence.
/// </summary>
public interface IAppPreferences
{
    /// <summary>
    /// Current preferences snapshot. Treated as immutable; a new instance is published on changes.
    /// </summary>
    AppPreferencesSnapshot Snapshot { get; }

    /// <summary>
    /// The device name as reported by the operating system.
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Changes the app theme mode and persists the value.
    /// No-op when the value is unchanged.
    /// </summary>
    Task ChangeThemeAsync(ThemeMode newTheme, CancellationToken ct = default);

    /// <summary>
    /// Changes the app language and persists the value.
    /// No-op when the value is unchanged.
    /// </summary>
    Task ChangeLanguageAsync(Language newLanguage, CancellationToken ct = default);

    /// <summary>
    /// Enables or disables notification preferences and persists the value.
    /// No-op when the value is unchanged.
    /// </summary>
    Task SetNotificationsEnabledAsync(bool enabled, CancellationToken ct = default);

    Task SetDeviceId(string id, CancellationToken ct = default);

    Task SetUserId(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Raised after a preference change with the updated snapshot.
    /// </summary>
    event EventHandler<AppPreferencesSnapshot> PreferencesChanged;
}
