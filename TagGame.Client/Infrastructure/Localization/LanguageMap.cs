using System.Globalization;
using TagGame.Client.Core.Options;

namespace TagGame.Client.Infrastructure.Localization;

/// <summary>
/// Maps the app's Language enum to CultureInfo instances.
/// </summary>
public static class LanguageMap
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de");

    public static CultureInfo ToCulture(Language language) => language switch
    {
        Language.German => German,
        Language.English => English,
        Language.System => IsSupportedLanguage(CultureInfo.CurrentCulture) ? CultureInfo.CurrentCulture : English,
        _ => English
    };

    private static bool IsSupportedLanguage(CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName switch
        {
            "de" => true,
            "en" => true,
            _ => false
        };
    }
}
