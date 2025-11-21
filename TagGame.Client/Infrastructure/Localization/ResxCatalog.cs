using System.Globalization;
using System.Resources;
using TagGame.Client.Core.Localization;

namespace TagGame.Client.Infrastructure.Localization;

public class ResxCatalog : ILocalizationCatalog
{
    private const string ResxNamespace = "TagGame.Client.Resources.Localization";
    private readonly ResourceManager[] _managers =
    [
        CreateManager("App"),
        CreateManager("Errors"),
        CreateManager("UserInit"),
        CreateManager("Start")
    ];

    public bool TryGet(string key, CultureInfo info, out string? value)
    {
        var managerName = key.Split('.')[0];
        var manager = _managers.FirstOrDefault(rm => rm.BaseName == GetPath(managerName));
        value = manager?.GetString(key, info);

        if (!string.IsNullOrEmpty(value))
            return true;

        value = null;
        return false;
    }

    private static string GetPath(string file) => ResxNamespace + "." + file;

    private static ResourceManager CreateManager(string file) => new(GetPath(file), typeof(ResxCatalog).Assembly);
}
