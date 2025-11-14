namespace TagGame.Client.Ui.Views;

/// <summary>
/// Base page that wraps normal XAML content and overlays a global ToastHost
/// (top, centered) plus a simple loading overlay. Derive your XAML pages from
/// this type and declare content normally inside &lt;base:PageBase&gt; ... &lt;/base:PageBase&gt;.
/// </summary>
public class PageBase : ContentPage
{
    private readonly ToastPresenter _toastPresenter;

    protected PageBase()
    {
        SafeAreaEdges = SafeAreaEdges.All;
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);

        _toastPresenter = GetRequiredService<ToastPresenter>();
        ControlTemplate = new ControlTemplate(CreateTemplate);
    }

    private Grid CreateTemplate()
    {
        var presenter = new ContentPresenter();

        var host = _toastPresenter.ToastHost;
        if (host.Parent is Layout layout)
            layout.Children.Remove(host);

        host.HorizontalOptions = LayoutOptions.Center;
        host.VerticalOptions = LayoutOptions.Start;
        host.Margin = new Thickness(0);

        var root = new Grid();
        root.Children.Add(presenter);
        root.Children.Add(host);

        return root;
    }

    protected internal static TService? GetService<TService>() =>
        Application.Current!.Handler!.MauiContext!.Services.GetService<TService>();

    protected internal static TService GetRequiredService<TService>() where TService : notnull =>
        Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<TService>();
}
