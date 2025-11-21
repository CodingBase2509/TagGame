using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Ui.Components.Toasts;

public partial class Toast : Border
{
    private const string ToastStylePrefix = "Toast";

    public required string Text { get; init; }

    public required ToastType Type { get; init; }

    public Toast()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public void Apply()
    {
        var styleName = $"{ToastStylePrefix}{Type}";
        if (Application.Current!.Resources.TryGetValue(styleName, out var style))
        {
            Style = style as Style;
        }
        TextLabel.Text = Text;
        ConfigureIcon(Type);
    }

    private void ConfigureIcon(ToastType type)
    {
        IconPath.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(GetIconPath(type))!;

        if (type == ToastType.Success)
        {
            IconPath.Stroke = TextLabel.TextColor;
            IconPath.Fill = Colors.Transparent;
        }
        else
        {
            IconPath.Stroke = Colors.Transparent;
            IconPath.Fill = TextLabel.TextColor;
        }
    }

    private static string GetIconPath(ToastType type) => type switch
    {
        ToastType.Success => "M4 12.6111L8.92308 17.5L20 6.5",
        ToastType.Info => "M12 7C12.8284 7 13.5 6.32843 13.5 5.5C13.5 4.67157 12.8284 4 12 4C11.1716 4 10.5 4.67157 10.5 5.5C10.5 6.32843 11.1716 7 12 7ZM11 9C10.4477 9 10 9.44772 10 10C10 10.5523 10.4477 11 11 11V19C11 19.5523 11.4477 20 12 20C12.5523 20 13 19.5523 13 19V10C13 9.44772 12.5523 9 12 9H11Z",
        ToastType.Warning => "M12 7C12.8284 7 13.5 6.32843 13.5 5.5C13.5 4.67157 12.8284 4 12 4C11.1716 4 10.5 4.67157 10.5 5.5C10.5 6.32843 11.1716 7 12 7ZM11 9C10.4477 9 10 9.44772 10 10C10 10.5523 10.4477 11 11 11V19C11 19.5523 11.4477 20 12 20C12.5523 20 13 19.5523 13 19V10C13 9.44772 12.5523 9 12 9H11Z",
        ToastType.Error => "M18.8 16L24.3 10.5C25.1 9.7 25.1 8.5 24.3 7.7 23.9 7.3 23.5 7 23 7 22.5 7 22 7.2 21.6 7.6L16 13.2 10.5 7.7C9.7 6.9 8.4 6.9 7.7 7.6 7.3 8 7 8.5 7 9.1 7 9.6 7.2 10.1 7.6 10.5L13.1 16 7.6 21.5C7.2 21.9 7 22.4 7 23 7 23.5 7.2 24 7.6 24.4 8 24.8 8.5 25 9 25 9.5 25 10 24.8 10.4 24.4L16 18.8 21.5 24.3C22.3 25.1 23.6 25.1 24.3 24.4 25.1 23.6 25.1 22.3 24.3 21.6L18.8 16Z",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
