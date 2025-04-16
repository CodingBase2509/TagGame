using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace TagGame.Client;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// MainPage = new AppShell();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}

