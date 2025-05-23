using System.ComponentModel;

namespace TagGame.Client.Ui.Navigation;

public enum NavigationMode
{
    [Description(@"..")]
    Backward,
    [Description(@"//")]
    Parallel,
    [Description(@"/")]
    Forward
}