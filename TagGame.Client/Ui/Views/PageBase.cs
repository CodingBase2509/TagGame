using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Layouts;
using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public class PageBase : ContentPage
{
    private readonly ViewModelBase _viewModel;
    
    public Grid BaseLayout { get; private set; }
    
    public PageBase(ViewModelBase vm)
    {
        _viewModel = vm;
        On<iOS>().SetUseSafeArea(false);
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);

        Appearing += InitViewModelAsync;
        Disappearing += CleanViewModelAsync;
        ControlTemplate = CreateTemplate();
    }

    ~PageBase()
    {
        Appearing -= InitViewModelAsync;
        Disappearing -= CleanViewModelAsync;
    }

    private ControlTemplate CreateTemplate()
    {
        return new ControlTemplate(() =>
        {
            var presenter = new ContentPresenter();
            BaseLayout =
            [
                presenter
            ];
            
            return BaseLayout;
        });
    }

    
    private async void InitViewModelAsync(object? sender, EventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void CleanViewModelAsync(object? sender, EventArgs e)
    {
        await _viewModel.CleanUpAsync();
    }
}