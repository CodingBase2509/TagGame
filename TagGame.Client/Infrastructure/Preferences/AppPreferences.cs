using TagGame.Client.Core.Options;

namespace TagGame.Client.Infrastructure.Preferences;

public class AppPreferences(IPreferences preferences) : IAppPreferences, IDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _snapshotLoaded;

    public AppPreferencesSnapshot Snapshot
    {
        get
        {
            EnsureSnapshotLoaded();
            return field;
        }
        private set;
    } = new(ThemeMode.System, Language.System, false, string.Empty, Guid.Empty);

    public string DeviceName => DeviceInfo.Name;

    public event EventHandler<AppPreferencesSnapshot>? PreferencesChanged;

    public async Task ChangeThemeAsync(ThemeMode newTheme, CancellationToken ct = default)
    {
        EnsureSnapshotLoaded();

        if (Snapshot.ThemeMode == newTheme)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { ThemeMode = newTheme };
        preferences.Set(PreferenceKeys.Theme, (int)newTheme);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task ChangeLanguageAsync(Language newLanguage, CancellationToken ct = default)
    {
        EnsureSnapshotLoaded();

        if (Snapshot.Language == newLanguage)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { Language = newLanguage };
        preferences.Set(PreferenceKeys.Language, (int)newLanguage);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task SetNotificationsEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        EnsureSnapshotLoaded();

        if (Snapshot.NotificationsEnabled == enabled)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { NotificationsEnabled = enabled };
        preferences.Set(PreferenceKeys.NotificationsEnabled, enabled);

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public async Task SetDeviceId(string id, CancellationToken ct = default)
    {
        EnsureSnapshotLoaded();

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
        EnsureSnapshotLoaded();

        if (Snapshot.UserId == id)
            return;
        await _lock.WaitAsync(ct);

        Snapshot = Snapshot with { UserId = id };
        preferences.Set(PreferenceKeys.UserId, id.ToString());

        _lock.Release();
        PreferencesChanged?.Invoke(this, Snapshot);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _lock.Dispose();
    }

    private void EnsureSnapshotLoaded()
    {
        if (_snapshotLoaded)
            return;

        if (!preferences.ContainsKey(PreferenceKeys.Theme))
            preferences.Set(PreferenceKeys.Theme, (int)ThemeMode.System);

        if (!preferences.ContainsKey(PreferenceKeys.Language))
            preferences.Set(PreferenceKeys.Language, (int)Language.System);

        if (!preferences.ContainsKey(PreferenceKeys.NotificationsEnabled))
            preferences.Set(PreferenceKeys.NotificationsEnabled, false);

        if (!preferences.ContainsKey(PreferenceKeys.DeviceId))
            preferences.Set(PreferenceKeys.DeviceId, string.Empty);

        if (!preferences.ContainsKey(PreferenceKeys.UserId))
            preferences.Set(PreferenceKeys.UserId, Guid.Empty.ToString());

        var theme = (ThemeMode)preferences.Get(PreferenceKeys.Theme, (int)ThemeMode.System);
        var language = (Language)preferences.Get(PreferenceKeys.Language, (int)Language.System);
        var notificationsEnabled = preferences.Get(PreferenceKeys.NotificationsEnabled, false);
        var deviceId = preferences.Get(PreferenceKeys.DeviceId, string.Empty);
        var userIdString = preferences.Get(PreferenceKeys.UserId, Guid.Empty.ToString());
        var userId = Guid.Parse(userIdString);

        var snapshot = new AppPreferencesSnapshot(
            theme,
            language,
            notificationsEnabled,
            deviceId,
            userId
        );

        Snapshot = snapshot;
        _snapshotLoaded = true;
    }
}
