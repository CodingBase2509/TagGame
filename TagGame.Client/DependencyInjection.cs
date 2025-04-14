using TagGame.Client.Services;
using TagGame.Client.Ui;
using TagGame.Client.Ui.Extensions;
using TagGame.Client.Ui.ViewModels;
using TagGame.Client.Ui.Views;
using INavigation = TagGame.Client.Ui.INavigation;

namespace TagGame.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddContentPages(this IServiceCollection services)
    {
        services.AddTransient<InitPage>();
        services.AddTransient<InitPageViewModel>();
        
        services.AddTransient<StartPage>();
        services.AddTransient<StartPageVm>();
        
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<LocalizationExtension>();
        services.AddSingleton<INavigation, Navigation>();
        
        services.AddSingleton<ConfigHandler>();
        services.AddSingleton<Encryption>();
        
        return services;
    }

    public static IServiceCollection AddApiClients(this IServiceCollection services)
    {
        services.AddSingleton<RestClient>();
        services.AddSingleton<LobbyClient>();
        services.AddSingleton<GameClient>();
        
        return services;
    }
}