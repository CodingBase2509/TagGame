using TagGame.Client.Core.Ui.ViewModels;

namespace TagGame.Client.Ui.Views.Base;

/// <summary>
/// Base page that wraps normal XAML content and overlays a global ToastHost
/// (top, centered) plus a simple loading overlay. Derive your XAML pages from
/// this type and declare content normally inside &lt;base:PageBase&gt; ... &lt;/base:PageBase&gt;.
/// </summary>
public class PageBase : ContentPage
{
    private readonly ToastPresenter _toastPresenter;
    protected Grid? root;

    protected ViewModelBase? ViewModel { get; }

    protected PageBase()
    {
        SafeAreaEdges = SafeAreaEdges.All;
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);

        Padding = new Thickness(16);
        _toastPresenter = GetRequiredService<ToastPresenter>();
        ControlTemplate = new ControlTemplate(CreateTemplate);
    }

    protected PageBase(ViewModelBase vm) : this()
    {
        ArgumentNullException.ThrowIfNull(vm);
        ViewModel = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await InvokeViewModelAsync(async vm =>
        {
            await vm.EnsureInitializedAsync().ConfigureAwait(false);
            await vm.OnAppearingAsync().ConfigureAwait(false);
        });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        await InvokeViewModelAsync(vm => vm.OnDisappearingAsync());
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is null && ViewModel is not null)
            ViewModel.Dispose();
    }

    private Task InvokeViewModelAsync(Func<ViewModelBase, Task> invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        return ViewModel is null ? Task.CompletedTask : invoker(ViewModel);
    }

    private Grid CreateTemplate()
    {
        var presenter = new ContentPresenter();

        var toastHost = _toastPresenter.ToastHost;
        if (toastHost.Parent is Layout layout)
            layout.Children.Remove(toastHost);

        toastHost.HorizontalOptions = LayoutOptions.Center;
        toastHost.VerticalOptions = LayoutOptions.Start;
        toastHost.Margin = new Thickness(0);

        root = new Grid
        {
            Children = { presenter, toastHost }
        };

        return root;
    }

    private static TService GetRequiredService<TService>() where TService : notnull =>
        Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<TService>();
}
