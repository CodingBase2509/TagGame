using System.Globalization;

namespace TagGame.Client.Core.Localization;

public interface ILocalizer
{
    /// <summary>
    /// Returns a localized string formatted with the given arguments.
    /// </summary>
    /// <param name="key">The key of the localization entry.</param>
    /// <param name="args">The arguments to format the string with.</param>
    /// <returns>The formatted string.</returns>
    string GetFormat(string key, params object[] args);

    /// <summary>
    /// Direct access view path: 'app.title'.
    /// </summary>
    /// <param name="key">The key of the localization entry.</param>
    string GetString(string key);

    CultureInfo CurrentCulture { get; }

    Task SetCultureAsync(CultureInfo culture);

    event EventHandler CultureChanged;
}
