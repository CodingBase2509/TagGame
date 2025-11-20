using TagGame.Client.Core.Ui.ViewModels.Start;

namespace TagGame.Client.Ui.Views.Start;

public partial class Start : PageWithModal
{
    public Start(StartViewModel vm)
        : base(vm)
    {
        InitializeComponent();
    }
}

