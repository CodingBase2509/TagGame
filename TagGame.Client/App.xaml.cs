using TagGame.Client.Services;
using TagGame.Client.Ui.ViewModels;
using TagGame.Client.Ui.Views;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Client;

public partial class App : Application
{
	public App(IServiceProvider services)
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}

