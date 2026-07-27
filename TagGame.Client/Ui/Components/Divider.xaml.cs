namespace TagGame.Client.Ui.Components;

public partial class Divider : BoxView
{
    public enum DividerOrientation
    {
        Horizontal,
        Vertical
    }

    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation),
        typeof(DividerOrientation),
        typeof(Divider),
        DividerOrientation.Horizontal,
        propertyChanged: OnOrientationChanged);

    public Divider()
    {
        InitializeComponent();
        ApplyOrientation(Orientation);
    }

    public DividerOrientation Orientation
    {
        get => (DividerOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Divider divider)
            return;

        divider.ApplyOrientation((DividerOrientation)newValue);
    }

    private void ApplyOrientation(DividerOrientation orientation)
    {
        switch (orientation)
        {
            case DividerOrientation.Horizontal:
                WidthRequest = -1;
                HeightRequest = 1;
                HorizontalOptions = LayoutOptions.Fill;
                VerticalOptions = LayoutOptions.Center;
                break;
            case DividerOrientation.Vertical:
                WidthRequest = 1;
                HeightRequest = -1;
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Fill;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
        }
    }
}
