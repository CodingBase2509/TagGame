using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagGame.Client.Ui.ViewModels;

namespace TagGame.Client.Ui.Views;

public partial class LobbyPage : PageBase
{
    public LobbyPage(LobbyPageVm vm)
        : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}