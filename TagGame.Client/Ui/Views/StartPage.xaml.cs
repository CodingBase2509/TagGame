using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public partial class StartPage : PageBase
{
    public StartPage(StartPageVm vm)
        : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}