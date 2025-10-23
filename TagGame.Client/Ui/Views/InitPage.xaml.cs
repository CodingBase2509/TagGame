using TagGame.Client.Ui.ViewModels;
using TagGame.Client.Services;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.Views;

public partial class InitPage : PageBase
{
    public InitPage() : this(ServiceHelper.GetRequiredService<InitPageViewModel>())
    {
    }

    public InitPage(InitPageViewModel vm) 
        : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
