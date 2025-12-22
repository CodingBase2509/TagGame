namespace TagGame.Client.Core.Options;

/// <summary>
/// Supported UI languages (extend as needed).
/// </summary>
public enum Language
{
    /// <summary>System Lang.</summary>
    System = 0,
    /// <summary>English (en).</summary>
    English = 1,
    /// <summary>German (de).</summary>
    German = 2,
}

public static class LanguageExtensions
{
    extension(Language lang)
    {
        public static IReadOnlyList<Language> Options() => Enum.GetValues<Language>();
    }
}
