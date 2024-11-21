using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TagGame.Client.Ui.Components;

public partial class AvatarButton : ContentView
{
    public static readonly BindableProperty AvatarColorProperty =
        BindableProperty.Create(
            nameof(Color),
            typeof(Color),
            typeof(AvatarButton),
            Colors.Transparent);

    public static readonly BindableProperty SelectedColorProperty =
        BindableProperty.Create(
            nameof(SelectedColor),
            typeof(Color),
            typeof(AvatarButton),
            propertyChanged: OnSelectedColorChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(AvatarButton),
            null);

    public Color Color
    {
        get => (Color)GetValue(AvatarColorProperty);
        set => SetValue(AvatarColorProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public AvatarButton()
    {
        InitializeComponent();
    }

    private static void OnSelectedColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AvatarButton button)
            button.UpdateBackgroundColor();
    }

    private void UpdateBackgroundColor()
    {
        Button.BackgroundColor = Equals(SelectedColor, Color) switch
        {
            true => Colors.Azure,
            false => Colors.Transparent,
        };
    }

}