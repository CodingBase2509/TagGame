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
    }

    public void Apply()
    {
        var styleName = $"{ToastStylePrefix}{Type}";
        var style = Resources[styleName] as Style;
        Style = style;
        TextLabel.Text = Text ?? string.Empty;
        Icon.Source = Type switch
        {
            ToastType.Success => "check.svg",
            ToastType.Warning => "info.svg",
            ToastType.Error => "cross.svg",
            ToastType.Info => "info.svg",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

