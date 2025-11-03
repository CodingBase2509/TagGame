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

        builder.Services.AddInfrastructure();
        builder.Services.AddServices(builder.Configuration);

        builder.Services.AddJsonOptionsProvider();
        builder.Services.AddNetworkingResilience();
        builder.Services.AddHttpServices(builder.Configuration, config =>
        {
#if IOS
            return config["Api:BaseAddress"];
#elif ANDROID
            return config["Api:BaseUrl"];
#else
            return "http://localhost:5240";
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
