using TagGame.Client.Core.Ui.ViewModels.Settings;

namespace TagGame.Client.Ui.Views.Settings;

public partial class SettingsPage : PageWithModal
{
    public SettingsPage(SettingsViewModel vw)
        : base(vw)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Shell.SetNavBarIsVisible(this, false);
    }
}

