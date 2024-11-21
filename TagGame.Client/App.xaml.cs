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

		if (VersionTracking.IsFirstLaunchEver)
		{
			var initPage = services.GetRequiredService<InitPage>();
			MainPage = initPage;
			var vm = initPage.BindingContext as InitPageViewModel;
			if (vm is null)
				return;
			vm!.InitCompleted += (s, e) => MainPage = new AppShell();
		}
		else
			MainPage = new AppShell();
	}
}

