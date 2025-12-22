namespace TagGame.Client.Core.Options;

/// <summary>
/// Preferred application theme mode.
/// </summary>
public enum ThemeMode
{
    /// <summary>Follow the system theme.</summary>
    System = 0,
    /// <summary>Light theme.</summary>
    Light = 1,
    /// <summary>Dark theme.</summary>
    Dark = 2,
}

public static class ThemeModeExtensions
{
    extension(ThemeMode mode)
    {
        public static IReadOnlyList<ThemeMode> Options() => Enum.GetValues<ThemeMode>();
    }
}
