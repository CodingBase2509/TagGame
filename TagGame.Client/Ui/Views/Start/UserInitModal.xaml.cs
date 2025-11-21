using TagGame.Client.Core.Ui.ViewModels.Start;

namespace TagGame.Client.Ui.Views.Start;

public partial class UserInitModal : ModalBase
{
    public UserInitModal(UserInitViewModel vm)
        : base(vm)
    {
        InitializeComponent();
    }
}

