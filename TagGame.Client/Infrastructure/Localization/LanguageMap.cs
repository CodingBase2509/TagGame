using System.Globalization;
using TagGame.Client.Core.Options;

namespace TagGame.Client.Infrastructure.Localization;

/// <summary>
/// Maps the app's Language enum to CultureInfo instances.
/// </summary>
public static class LanguageMap
{
    public static CultureInfo ToCulture(Language language) => language switch
    {
        Language.German => new CultureInfo("de"),
        Language.English => new CultureInfo("en"),
        Language.System => IsSupportedLanguage(CultureInfo.CurrentCulture) ? CultureInfo.CurrentCulture : new CultureInfo("en"),
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

