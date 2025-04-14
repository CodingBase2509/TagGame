using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TagGame.Client.Services;

public static class Localization
{
    private static readonly Dictionary<string, ResourceManager> _resourceManagers = new()
    {
        { "StartPage", new ResourceManager("TagGame.Client.Resources.Localization.StartPage", 
            typeof(Localization).Assembly) },
        { "InitPage", new ResourceManager("TagGame.Client.Resources.Localization.InitPage",
            typeof(Localization).Assembly)}
    };

    public static string Get(string key, string pageName)
    {
        if (_resourceManagers.TryGetValue(pageName, out var resourceManager))
        {
            return resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
        }
        return $"[{key}]";
    }
}