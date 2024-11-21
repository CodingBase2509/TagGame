using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public partial class InitPage : PageBase
{
    public InitPage(InitPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        Appearing += async (s, e) => await viewModel.IsInitializedAsync();
    }
}