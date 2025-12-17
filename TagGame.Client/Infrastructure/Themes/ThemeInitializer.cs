using TagGame.Client.Core.Options;

namespace TagGame.Client.Infrastructure.Themes;

public class ThemeInitializer(IAppPreferences preferences)
{
    private int _initialized;

    public Task InitializeAsync(CancellationToken ct = default)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return Task.CompletedTask;

        Apply(preferences.Snapshot.ThemeMode);

        preferences.PreferencesChanged += OnPreferenceChanged;
        ct.Register(() => preferences.PreferencesChanged -= OnPreferenceChanged);

        return Task.CompletedTask;
    }

    private void Apply(ThemeMode theme) =>
        Application.Current?.UserAppTheme = ThemeMap.ToTheme(theme);

    private void OnPreferenceChanged(object? sender, AppPreferencesSnapshot snap) =>
        Apply(snap.ThemeMode);
}
