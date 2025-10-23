using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using TagGame.Client.Services;

namespace TagGame.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiMaps()
			.ConfigureEssentials(essentials =>
			{
				essentials.UseVersionTracking();
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.ConfigureHandlers();
		
		// register views and view models
		builder.Services.AddContentPages();
		
		// register services
		builder.Services.AddServices();
		builder.Services.AddApiClients();
		builder.Services.ConfigureJsonOptions();
		
#if ANDROID
		builder.CustomizeEntryHandlers();
#endif
		
#if DEBUG
		builder.Logging.AddDebug();
#endif
		var app = builder.Build();
		ServiceHelper.SetProvider(app.Services);

		return app;
	}
}

