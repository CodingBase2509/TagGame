using TagGame.Client.Ui.ViewModels;
using TagGame.Client.Services;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.Views;

public partial class StartPage : PageBase
{
    public StartPage() : this(ServiceHelper.GetRequiredService<StartPageVm>())
    {
    }

    public StartPage(StartPageVm vm)
        : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
