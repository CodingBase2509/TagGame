using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TagGame.Client.Ui;

public class Navigation : INavigation
{
    public async Task GoToLobby(NavigationMode mode, Dictionary<string, object>? navItems = null) =>
        await GoToAsync("lobby", mode, navItems);
    
    public async Task GoToStart(NavigationMode mode, Dictionary<string, object>? navItems = null) =>
        await GoToAsync("start", mode, navItems);
    
    public async Task GoToInit(NavigationMode mode, Dictionary<string, object>? navItems = null) =>
        await GoToAsync("init", mode, navItems);
    
    private static async Task GoToAsync(string page, NavigationMode mode, Dictionary<string, object>? navItems = null)
    {
        string navMode = mode.GetDescription();
        string navDest = mode == NavigationMode.Backward ? navMode : $@"{navMode}{page}";

        if (navItems is null)
            await Shell.Current.GoToAsync(navDest, true);
        else
            await Shell.Current.GoToAsync(navDest, true, navItems);
    }
}