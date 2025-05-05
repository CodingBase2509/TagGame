using System.ComponentModel;

namespace TagGame.Client.Ui;

public enum NavigationMode
{
    [Description(@"../")]
    Backward,
    [Description(@"//")]
    Parallel,
    [Description(@"/")]
    Forward
}