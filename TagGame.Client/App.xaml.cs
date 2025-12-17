using TagGame.Client.Infrastructure;

namespace TagGame.Client;

public partial class App : Application, IDisposable
{
    private readonly Init _init;
    public App(Init init)
    {
        _init = init;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _init.InitServices();
        return new Window(new AppShell());
    }

    public void Dispose()
    {
        _init.Dispose();
        GC.SuppressFinalize(this);
    }
}
