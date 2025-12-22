using TagGame.Client.Core.Features.Users;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Client.Core.Ui.ViewModels.Settings;

public partial class SettingsViewModel(IAppPreferences prefs, IUserApi api) : ViewModelBase
{
    public static IReadOnlyList<ThemeMode> ThemeModes => ThemeMode.Options();
    public static IReadOnlyList<Language> Languages => Language.Options();

    [ObservableProperty]
    private UserProfileDto? _currentUser;

    [ObservableProperty]
    private ThemeMode _theme;

    [ObservableProperty]
    private Language _language;

    [ObservableProperty]
    private bool _notificationsEnabled;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        CurrentUser = await api.GetProfileAsync(ViewModelCancellationToken);

    public async Task UpdateTheme() => await prefs.ChangeThemeAsync(Theme, ViewModelCancellationToken);

    public async Task UpdateLanguage() => await prefs.ChangeLanguageAsync(Language, ViewModelCancellationToken);

    public async Task UpdateNotifications() =>
        await prefs.SetNotificationsEnabledAsync(NotificationsEnabled, ViewModelCancellationToken);
}
