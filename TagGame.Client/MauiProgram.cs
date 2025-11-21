using TagGame.Client.Core.Extensions;
using TagGame.Client.Core.Services;
using TagGame.Client.Extensions;
using TagGame.Client.Infrastructure;

namespace TagGame.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.ConfigureBuilder();

        builder.Configuration.AddAppSettingsFile();
        builder.Services.AddInfrastructure();
        builder.Services.AddPages();
        builder.Services.AddViewModels();

        builder.Services.AddCoreServices(builder.Configuration);
        builder.Services.AddJsonOptionsProvider();
        builder.Services.AddNetworkingResilience();
        builder.Services.AddHttpServices(builder.Configuration, config =>
        {
#if IOS
            return config["Api:BaseAddress"];
#elif ANDROID
            return config["Api:BaseUrl"];
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();
        SpUtils.Set(app.Services);
        return app;
    }
}
