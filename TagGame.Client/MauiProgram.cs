﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using DotNet.Meteor.HotReload.Plugin;
using TagGame.Client.Ui.Views;

namespace TagGame.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureEssentials(essentials =>
			{
				essentials.UseVersionTracking();
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// register views and view models
		builder.Services.AddContentPages();
		
		// register services
		builder.Services.AddServices();
		builder.Services.AddApiClients();
		
#if DEBUG
		builder.Logging.AddDebug();
		builder.EnableHotReload();
#endif
		
		return builder.Build();
	}
}

