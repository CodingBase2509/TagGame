using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;

namespace TagGame.Client.Infrastructure.Preferences;

public class AppPreferences(IPreferences preferences) : IAppPreferences
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AppPreferencesSnapshot Snapshot { get; private set; } = new(
        preferences.Get(PreferenceKeys.Theme, ThemeMode.System),
        preferences.Get(PreferenceKeys.Language, Language.English),
        preferences.Get(PreferenceKeys.NotificationsEnabled, false),
        preferences.Get(PreferenceKeys.DeviceId, Guid.Empty),
        preferences.Get(PreferenceKeys.UserId, Guid.Empty)
    );

    public event EventHandler<AppPreferencesSnapshot>? PreferencesChanged;

    public async Task ChangeThemeAsync(ThemeMode newTheme, CancellationToken ct = default)
    {
        if (Snapshot.ThemeMode == newTheme)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { ThemeMode = newTheme };
        preferences.Set(PreferenceKeys.Theme, newTheme);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task ChangeLanguageAsync(Language newLanguage, CancellationToken ct = default)
    {
        if (Snapshot.Language == newLanguage)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { Language = newLanguage };
        preferences.Set(PreferenceKeys.Language, newLanguage);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task SetNotificationsEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        if (Snapshot.NotificationsEnabled == enabled)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { NotificationsEnabled = enabled };
        preferences.Set(PreferenceKeys.NotificationsEnabled, enabled);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task SetDeviceId(Guid id, CancellationToken ct = default)
    {
        if (Snapshot.DeviceId == id)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { DeviceId = id };
        preferences.Set(PreferenceKeys.DeviceId, id);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task SetUserId(Guid id, CancellationToken ct = default)
    {
        if (Snapshot.UserId == id)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { UserId = id };
        preferences.Set(PreferenceKeys.UserId, id);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }
}
