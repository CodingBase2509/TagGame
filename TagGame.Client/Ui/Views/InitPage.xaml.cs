using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public partial class InitPage : PageBase
{
    public InitPage(InitPageViewModel vm) 
        : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}