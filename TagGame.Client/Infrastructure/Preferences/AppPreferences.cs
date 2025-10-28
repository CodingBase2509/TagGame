using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;
using MauiPreferences = Microsoft.Maui.Storage.Preferences;

namespace TagGame.Client.Infrastructure.Preferences;

public class AppPreferences : IAppPreferences
{
    private const int LockEnterWaitMs = 50;
    private readonly Lock _lock = new();

    public AppPreferencesSnapshot Snapshot { get; private set; } = new(
        MauiPreferences.Default.Get(PreferenceKeys.Theme, ThemeMode.System),
        MauiPreferences.Default.Get(PreferenceKeys.Language, Language.English),
        MauiPreferences.Default.Get(PreferenceKeys.NotificationsEnabled, false)
    );

    public event EventHandler<AppPreferencesSnapshot>? PreferencesChanged;

    public Task ChangeThemeAsync(ThemeMode newTheme)
    {
        if (!_lock.TryEnter(LockEnterWaitMs) || Snapshot.ThemeMode == newTheme)
            return Task.CompletedTask;

        Snapshot = Snapshot with { ThemeMode = newTheme };
        MauiPreferences.Default.Set(PreferenceKeys.Theme, newTheme);

        _lock.Exit();
        PreferencesChanged?.Invoke(this, Snapshot);

        return Task.CompletedTask;
    }

    public Task ChangeLanguageAsync(Language newLanguage)
    {
        if (!_lock.TryEnter(LockEnterWaitMs) || Snapshot.Language == newLanguage)
            return Task.CompletedTask;

        Snapshot = Snapshot with { Language = newLanguage };
        MauiPreferences.Default.Set(PreferenceKeys.Language, newLanguage);

        _lock.Exit();
        PreferencesChanged?.Invoke(this, Snapshot);

        return Task.CompletedTask;
    }

    public Task SetNotificationsEnabledAsync(bool enabled)
    {
        if (!_lock.TryEnter(LockEnterWaitMs) || Snapshot.NotificationsEnabled == enabled)
            return Task.CompletedTask;

        Snapshot = Snapshot with { NotificationsEnabled = enabled };
        MauiPreferences.Default.Set(PreferenceKeys.NotificationsEnabled, enabled);

        _lock.Exit();
        PreferencesChanged?.Invoke(this, Snapshot);

        return Task.CompletedTask;
    }
}
