using Microsoft.Maui.Controls;
using TagGame.Client.Ui.Components.Toasts;

namespace TagGame.Client.Ui.Views.Base;

/// <summary>
/// Base page that wraps normal XAML content and overlays a global ToastHost
/// (top, centered) plus a simple loading overlay. Derive your XAML pages from
/// this type and declare content normally inside &lt;base:PageBase&gt; ... &lt;/base:PageBase&gt;.
/// </summary>
public class PageBase : ContentPage
{
    private Grid? _wrapper;
    private Grid? _loadingOverlay;

    protected void ShowLoading(string? text = null)
    {
        EnsureWrapped();
        if (_loadingOverlay is null) return;
        if (_loadingOverlay.Children.FirstOrDefault() is Border b && b.Content is HorizontalStackLayout hs && hs.Children.OfType<Label>().FirstOrDefault() is Label lbl)
            lbl.Text = string.IsNullOrWhiteSpace(text) ? "Loading…" : text;
        _loadingOverlay.IsVisible = true;
        _loadingOverlay.InputTransparent = false;
    }

    protected void HideLoading()
    {
        if (_loadingOverlay is null) return;
        _loadingOverlay.IsVisible = false;
        _loadingOverlay.InputTransparent = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EnsureWrapped();
    }

    private void EnsureWrapped()
    {
        if (_wrapper is not null) return;

        // Capture original content
        var original = Content;
        _wrapper = new Grid();
        if (original is not null)
        {
            Content = null; // detach to avoid multiple parents
            _wrapper.Children.Add(original);
        }

        // Add global ToastHost (singleton) on top
        var host = this.Handler?.MauiContext?.Services?.GetService(typeof(ToastHost)) as ToastHost
                   ?? Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(ToastHost)) as ToastHost;
        if (host is not null)
        {
            // Reparent if necessary
            if (host.Parent is Layout<View> oldParent)
                oldParent.Children.Remove(host);
            _wrapper.Children.Add(host);
        }

        // Loading overlay
        _loadingOverlay = BuildLoadingOverlay();
        _wrapper.Children.Add(_loadingOverlay);

        Content = _wrapper;
    }

    private static Grid BuildLoadingOverlay()
    {
        var label = new Label { Text = "Loading…", TextColor = Colors.White, FontSize = 14 };
        var spinner = new ActivityIndicator { IsRunning = true, Color = Colors.White, WidthRequest = 28, HeightRequest = 28 };
        var card = new Border
        {
            Background = new SolidColorBrush(new Color(0.15f, 0.15f, 0.2f, 0.95f)),
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = 16,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = new HorizontalStackLayout { Spacing = 12, Children = { spinner, label } }
        };

        return new Grid
        {
            IsVisible = false,
            InputTransparent = true,
            BackgroundColor = new Color(0, 0, 0, 0.35f),
            Children = { card }
        };
    }
}

