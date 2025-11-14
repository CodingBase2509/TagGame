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

