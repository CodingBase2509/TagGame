using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Ui.Views.Start;

public partial class StartPage : PageBase
{
    private readonly IToastPublisher _toast;

    public StartPage(ToastPresenter presenter, IToastPublisher toast) : base(presenter)
    {
        _toast = toast;
        InitializeComponent();
    }

    private void Button_OnClicked(object? sender, EventArgs e) =>
        _toast.Info("Test 1234 Test 1234 Test 1234", false);
}

