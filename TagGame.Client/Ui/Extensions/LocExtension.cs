using System.Globalization;
using TagGame.Client.Core.Localization;

namespace TagGame.Client.Ui.Extensions;

[ContentProperty(nameof(Key))]
public sealed class LocExtension : IMarkupExtension<string>
{
    public string Key { get; set; } = string.Empty;

    public string? Args { get; set; }

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        var localizer = SpUtils.GetRequiredService<ILocalizer>();
        var args = ParseArgs(Args);
        return args.Length > 0
            ? localizer.GetFormat(Key, args)
            : localizer.GetString(Key);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

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
