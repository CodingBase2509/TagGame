using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace TagGame.Client.Ui.Views;

public class PageBase : ContentPage
{
    public PageBase()
    {
        On<iOS>().SetUseSafeArea(false);
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
    }
}