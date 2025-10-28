using TagGame.Client.Core.Options;

namespace TagGame.Client.Core.Services.Abstractions;

public interface IAppPreferences
{
    AppPreferencesSnapshot Snapshot { get; }

    Task ChangeThemeAsync(ThemeMode newTheme);

    Task ChangeLanguageAsync(Language newLanguage);

    Task SetNotificationsEnabledAsync(bool enabled);

    event EventHandler<AppPreferencesSnapshot> PreferencesChanged;
}
