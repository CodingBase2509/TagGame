namespace TagGame.Client.Core.Options;

public record class AppPreferencesSnapshot(
    ThemeMode ThemeMode,
    Language Language,
    bool NotificationsEnabled);
