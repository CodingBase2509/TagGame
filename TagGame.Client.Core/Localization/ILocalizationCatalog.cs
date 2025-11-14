using System.Globalization;

namespace TagGame.Client.Core.Localization;

public interface ILocalizationCatalog
{
    bool TryGet(string key, CultureInfo info, out string? value);
}
