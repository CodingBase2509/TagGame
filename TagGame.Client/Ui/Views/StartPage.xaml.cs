using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public partial class StartPage : PageBase
{
    public StartPage(StartPageVm vm)
    {
        InitializeComponent();
        BindingContext = vm;

        Appearing += async (s, e) => await vm.InitializeAsync();
    }
}