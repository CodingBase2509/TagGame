using TagGame.Client.Ui.ToastMessages;
using Application = Microsoft.Maui.Controls.Application;

namespace TagGame.Client;

public partial class App : Application
{
	private readonly IToastService _toast;
	
	public App(IToastService toast)
	{
		InitializeComponent();
		_toast = toast;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell(_toast));
	}
}

