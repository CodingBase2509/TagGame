using TagGame.Client.Core.Options;

namespace TagGame.Client.Infrastructure.Themes;

public class ThemeMap
{
    public static AppTheme ToTheme(ThemeMode theme) => theme switch
    {
        ThemeMode.System => AppTheme.Unspecified,
        ThemeMode.Light => AppTheme.Light,
        ThemeMode.Dark => AppTheme.Dark,
        _ => AppTheme.Unspecified
    };
}
