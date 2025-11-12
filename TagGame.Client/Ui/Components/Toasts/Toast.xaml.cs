using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Ui.Components.Toasts;

public partial class Toast : Border
{
    public required string Text { get; init; }

    public required ToastType Type { get; init; }

    public Toast()
    {
        InitializeComponent();
    }

    public void Apply()
    {
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

