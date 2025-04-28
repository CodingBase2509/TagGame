using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public class PageBase : ContentPage
{
    private readonly ViewModelBase _viewModel;
    
    public PageBase(ViewModelBase vm)
    {
        _viewModel = vm;
        On<iOS>().SetUseSafeArea(false);
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);

        Appearing += InitViewModelAsync;
        Disappearing += CleanViewModelAsync;
    }

    ~PageBase()
    {
        Appearing -= InitViewModelAsync;
        Disappearing -= CleanViewModelAsync;
    }

    private async void InitViewModelAsync(object? sender, EventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void CleanViewModelAsync(object? sender, EventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}