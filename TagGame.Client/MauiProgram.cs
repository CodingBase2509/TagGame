using TagGame.Client.Core.Extensions;
using TagGame.Client.Extensions;
using TagGame.Client.Infrastructure;

namespace TagGame.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Configuration.AddAppSettingsFile();

        builder.Services.AddJsonOptionsProvider()
            .AddNetworkingResilience()
            .AddHttpServices(builder.Configuration, config =>
            {
#if IOS
                return config["Api:BaseAddress"];
#elif ANDROID
                return config["Api:BaseUrl"];
#else
                return "http://localhost:5240";
#endif
            });

        builder.Services.AddInfrastructure();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
