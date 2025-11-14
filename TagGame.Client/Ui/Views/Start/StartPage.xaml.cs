using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Ui.Views.Start;

public partial class StartPage : PageBase
{
    private readonly IToastPublisher _toast;

    public StartPage(IToastPublisher toast)
    {
        _toast = toast;
        InitializeComponent();
    }

    private void Button_OnClicked(object? sender, EventArgs e) =>
        _toast.Info("Test 1234 Test 1234 Test 1234", false);

    private void Button_OnClicked2(object? sender, EventArgs e) =>
        _toast.Error("Test 1234 Test 1234 Test 1234", false);
}

