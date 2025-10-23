using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public partial class RoomSettingsModal : PageBase
{
    public RoomSettingsModal(RoomSettingsModalVm vm)
        :base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}