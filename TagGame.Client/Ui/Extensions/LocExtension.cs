using System.Globalization;
using TagGame.Client.Core.Localization;

namespace TagGame.Client.Ui.Extensions;

[ContentProperty(nameof(Key))]
public sealed class LocExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public string? Args { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var localizer = ResolveLocalizer(serviceProvider);
        var valueSource = new LocalizedValue(Key, ParseArgs(Args), localizer);
        return new Binding(nameof(LocalizedValue.Value), source: valueSource, mode: BindingMode.OneWay);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

    private static ILocalizer? ResolveLocalizer(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(ILocalizer)) is ILocalizer direct)
            return direct;

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt &&
            pvt.TargetObject is Element element)
        {
            return element.Handler?.MauiContext?.Services.GetService<ILocalizer>();
        }

        return Application.Current?.Handler?.MauiContext?.Services.GetService<ILocalizer>();
    }

    private static object[] ParseArgs(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        var tokens = raw.Split('|', StringSplitOptions.TrimEntries);
        var values = new List<object>(tokens.Length);
        foreach (var token in tokens)
        {
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            {
                values.Add(i);
                continue;
            }

            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
            {
                values.Add(l);
                continue;
            }

            if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var m))
            {
                values.Add(m);
                continue;
            }

            if (double.TryParse(token,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture,
                    out var d))
            {
                values.Add(d);
                continue;
            }

            if (bool.TryParse(token, out var b))
            {
                values.Add(b);
                continue;
            }

            values.Add(token);
        }

        return [.. values];
    }
}

internal sealed class LocalizedValue : BindableObject, IDisposable
{
    private readonly object[] _args;
    private readonly ILocalizer? _localizer;

    public LocalizedValue(string key, object[] args, ILocalizer? localizer)
    {
        Value = key;
        _args = args;
        _localizer = localizer;

        if (_localizer is not null)
            _localizer.CultureChanged += OnCultureChanged;
    }

    public string Value
    {
        get
        {
            if (string.IsNullOrWhiteSpace(field))
                return string.Empty;

            if (_localizer is null)
                return $"[{field}]";

            return _args.Length > 0
                ? _localizer.GetFormat(field, _args)
                : _localizer.GetString(field);
        }
    }

    public void Dispose()
    {
        if (_localizer is null)
            return;

        _localizer.CultureChanged -= OnCultureChanged;
    }

    private void OnCultureChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Value));
}
