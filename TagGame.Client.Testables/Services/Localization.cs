using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TagGame.Client.Services;

public class Localization(Assembly assembly)
{
    private readonly Dictionary<string, ResourceManager> _resourceManagers = new()
    {
        { "StartPage", new ResourceManager("TagGame.Client.Resources.Localization.StartPage", assembly: assembly) },
        { "InitPage", new ResourceManager("TagGame.Client.Resources.Localization.InitPage", assembly: assembly)},
        { "LobbyPage", new ResourceManager("TagGame.Client.Resources.Localization.LobbyPage", assembly: assembly)}
    };

    public string Get(string key, string pageName)
    {
        if (_resourceManagers.TryGetValue(pageName, out var resourceManager))
        {
            return resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
        }
        return $"[{key}]";
    }
}