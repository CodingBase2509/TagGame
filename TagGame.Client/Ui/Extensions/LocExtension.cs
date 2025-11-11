using System.Globalization;
using TagGame.Client.Core.Localization;

namespace TagGame.Client.Ui.Extensions;

[ContentProperty(nameof(Key))]
public sealed class LocExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    // Optional: static argument
    public string? Args { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObject = pvt?.TargetObject as BindableObject;
        var src = new LocalizedValue(Key, Args, targetObject);
        return new Binding(nameof(LocalizedValue.Value), source: src, mode: BindingMode.OneWay);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

internal sealed class LocalizedValue(string key, string? args, BindableObject? target) : BindableObject, IDisposable
{
    private ILocalizer? _loc;
    private bool _subscribed;

    public string Value
    {
        get
        {
            var loc = ResolveLocalizer();
            if (loc is null)
                return $"[{key}]";

            var args1 = ParseArgs(args);
            return args1.Length > 0
                ? loc.GetFormat(key, args1)
                : loc.GetString(key);
        }
    }

    public void Dispose()
    {
        if (_loc is null)
            return;

        _loc.CultureChanged -= OnCultureChanged;
    }

    private ILocalizer? ResolveLocalizer()
    {
        if (_loc is not null)
            return _loc;

        var element = target as Element ?? Application.Current;
        var sp = element?.Handler?.MauiContext?.Services;
        if (sp is null)
            return null;

        _loc = sp.GetService<ILocalizer>();
        if (_loc is null || _subscribed)
            return _loc;

        _loc.CultureChanged += OnCultureChanged;
        _subscribed = true;
        return _loc;
    }

    private static object[] ParseArgs(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return [];

        var tokens = s.Split('|', StringSplitOptions.None | StringSplitOptions.TrimEntries);
        var list = new List<object>(tokens.Length);
        list.AddRange(tokens.Select(ConvertToken));
        return [.. list];
    }

    private static object ConvertToken(string token)
    {
        // Reihenfolge: int, long, decimal, double, bool → sonst string
        if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            return i;

        if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
            return l;

        if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var m))
            return m;

        if (double.TryParse(token, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture,
                out var d))
            return d;

        if (bool.TryParse(token, out var b))
            return b;
        return token;
    }

    private void OnCultureChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Value));
}
