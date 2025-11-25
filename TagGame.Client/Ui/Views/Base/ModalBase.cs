using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Shapes;
using TagGame.Client.Core.Ui.ViewModels;

namespace TagGame.Client.Ui.Views.Base;

public class ModalBase : ContentView, IModal
{
    private readonly INavigationService _navigation;
    private Border? _card;
    private BoxView? _backdrop;

    protected ViewModelBase? ViewModel { get; }

    public static readonly BindableProperty CardContentProperty = BindableProperty.Create(
        nameof(CardContent), typeof(View), typeof(ModalBase), propertyChanged: OnCardContentChanged);

    public static readonly BindableProperty CanCloseProperty = BindableProperty.Create(
        nameof(CanClose), typeof(bool), typeof(ModalBase), defaultValue: true);

    public View? CardContent
    {
        get => (View?)GetValue(CardContentProperty);
        set => SetValue(CardContentProperty, value);
    }

    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public ModalBase()
    {
        _navigation = SpUtils.GetRequiredService<INavigationService>();
        ConfigureModalDesign();
    }

    public ModalBase(ViewModelBase vm) : this()
    {
        ViewModel = vm;
        BindingContext = vm;
    }

    private void ConfigureModalDesign() => BuildLayout();

    // Lifecycle mirroring of PageBase
    public async Task OnShowAsync()
    {
        if (ViewModel is null)
            return;

        await AnimateInAsync();
        _ = ViewModel.EnsureInitializedAsync().ConfigureAwait(false);
        _ = ViewModel.OnAppearingAsync().ConfigureAwait(false);
    }

    public async Task OnHideAsync()
    {
        await AnimateOutAsync();

        if (ViewModel is not null)
            _ = ViewModel.OnDisappearingAsync();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is null)
            ViewModel?.Dispose();
    }

    private void BuildLayout()
    {
        var container = new Grid();
        Content = container;

        _backdrop = new BoxView
        {
            Color = Color.FromRgba(0, 0, 0, 0.4),
            Opacity = 0
        };
        container.Children.Add(_backdrop);

        _card = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = new Thickness(16),
            Margin = new Thickness(8),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.End,
            TranslationY = 800
        };
        if (Application.Current?.Resources
            .TryGetValue("ElevatedCard", out var elevatedCard) ?? false)
            _card.Style = elevatedCard as Style;

        container.Children.Add(_card);

        if (CardContent is not { } content)
        {
            return;
        }

        if (CanClose)
        {
            var backButton = new ImageButton
            {
                Source = "cross.svg",
                HeightRequest = 48,
                WidthRequest = 48,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                ZIndex = 1000,
                Command = new AsyncRelayCommand(async () => await _navigation.CloseModalAsync())
            };
            _card.Content = new Grid
            {
                Children = { backButton, content }
            };
        }
        else
        {
            _card.Content = content;
        }
    }

    private static void OnCardContentChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is ModalBase modal)
        {
            modal.BuildLayout();
        }
    }

    private async Task AnimateInAsync()
    {
        if (_card is null || _backdrop is null)
            return;

        await Task.WhenAll(
            _card.TranslateToAsync(0, 0, 500, Easing.CubicOut),
            _backdrop.FadeToAsync(1, 400, Easing.CubicOut));
        _card.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#33000000")),
            Opacity = 1f,
            Offset = new Point(0, 8),
            Radius = 12
        };
    }

    private async Task AnimateOutAsync()
    {
        if (_card is null || _backdrop is null)
            return;

        await Task.WhenAll(
            _card.TranslateToAsync(0, 800, 500, Easing.CubicIn),
            _backdrop.FadeToAsync(0, 400, Easing.CubicIn));
    }
}
