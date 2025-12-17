using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Infrastructure.Localization;

/// <summary>
/// Initializes and keeps the app's UI culture in sync with IAppPreferences.Language.
/// Subscribe once during app startup and call InitializeAsync().
/// </summary>
public sealed class LocalizationInitializer(ILocalizer localizer, IAppPreferences preferences)
{
    private int _initialized;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        await localizer.SetCultureAsync(LanguageMap.ToCulture(preferences.Snapshot.Language)).
            ConfigureAwait(false);

        preferences.PreferencesChanged += OnPreferenceChanged;
        ct.Register(() => preferences.PreferencesChanged -= OnPreferenceChanged);
    }

    private async void OnPreferenceChanged(object? sender, AppPreferencesSnapshot snap)
    {
        try
        {
            await localizer.SetCultureAsync(LanguageMap.ToCulture(snap.Language)).ConfigureAwait(false);
        }
        catch
        {
            // Intentionally swallow; localization updates should not crash the app.
        }
    }
}
