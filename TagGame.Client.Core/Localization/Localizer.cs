using System.Globalization;
using SmartFormat;
using SmartFormat.Extensions;
using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Localization;

public sealed class Localizer : ILocalizer
{
    private readonly ILocalizationCatalog _catalog;
    private readonly SmartFormatter _formatter;

    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

    public event EventHandler? CultureChanged;

    public Localizer(ILocalizationCatalog catalog)
    {
        _catalog = catalog;

        _formatter = Smart.CreateDefaultSmartFormat();
        _formatter.AddExtensions(new PluralLocalizationFormatter(), new ChooseFormatter(), new ConditionalFormatter(), new ListFormatter(), new TimeFormatter());
    }

    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (_catalog.TryGet(key, CurrentCulture, out var value) && !string.IsNullOrEmpty(value))
            return value;

        return $"[{key}]";
    }

    public string GetFormat(string key, params object[] args)
    {
        var template = GetString(key);
        // If key was missing, the template will be [key]; just return it as-is when no args provided
        return args is { Length: > 0 }
            ? _formatter.Format(CurrentCulture, template, args)
            : template;
    }

    public Task SetCultureAsync(CultureInfo culture)
    {
        CurrentCulture = culture;
        CultureInfo.CurrentUICulture = CurrentCulture;
        CultureInfo.CurrentCulture = CurrentCulture;
        CultureChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}

