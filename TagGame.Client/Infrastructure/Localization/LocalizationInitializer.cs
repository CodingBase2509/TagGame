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
    private bool _initialized;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized)
            return;
        _initialized = true;

        await localizer.SetCultureAsync(LanguageMap.ToCulture(preferences.Snapshot.Language));

        preferences.PreferencesChanged += OnPreferenceChanged;
        ct.Register(() => preferences.PreferencesChanged -= OnPreferenceChanged);
    }

    private void OnPreferenceChanged(object? sender, AppPreferencesSnapshot snap) =>
        localizer.SetCultureAsync(LanguageMap.ToCulture(snap.Language));
}

