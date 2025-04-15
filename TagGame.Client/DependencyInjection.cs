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
        
        services.AddSingleton<Encryption>();
        services.AddSingleton<ConfigHandler>(sp =>
        {
            var crypt = sp.GetRequiredService<Encryption>();
            var secureStorage = sp.GetRequiredService<ISecureStorage>();
            var configDir = FileSystem.Current.AppDataDirectory;
            
            return new ConfigHandler(crypt, secureStorage, configDir);
        });
     
        services.AddSingleton<ISecureStorage>(_ => SecureStorage.Default);
        
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