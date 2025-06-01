using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using TagGame.Client.Clients;
using TagGame.Client.Common;
using TagGame.Client.Handlers;
using TagGame.Client.Services;
using TagGame.Client.Ui.Extensions;
using TagGame.Client.Ui.ToastMessages;
using TagGame.Client.Ui.ViewModels;
using TagGame.Client.Ui.Views;
using TagGame.Client.Ui.Services;
using INavigation = TagGame.Client.Ui.Navigation.INavigation;

namespace TagGame.Client;

public static class DependencyInjection
{
    public static void AddContentPages(this IServiceCollection services)
    {
        services.AddTransient<InitPage>();
        services.AddTransient<InitPageViewModel>();
        
        services.AddTransient<StartPage>();
        services.AddTransient<StartPageVm>();

        services.AddTransient<LobbyPage>();
        services.AddTransient<LobbyPageVm>();
        Routing.RegisterRoute("lobby", typeof(LobbyPage));

        services.AddTransient<RoomSettingsModal>();
        services.AddTransient<RoomSettingsModalVm>();
        Routing.RegisterRoute("roomSettings", typeof(RoomSettingsModal));
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<INavigation, Navigation>();
        services.AddSingleton<ISecureStorage>(_ => SecureStorage.Default);
        
        services.AddSingleton<Localization>(_ => new Localization(typeof(App).Assembly));
        services.AddSingleton<LocalizationExtension>();
        
        services.AddSingleton<Encryption>();
        services.AddSingleton<ConfigHandler>(sp =>
        {
            var crypt = sp.GetRequiredService<Encryption>();
            var secureStorage = sp.GetRequiredService<ISecureStorage>();
            var jsonOptions = sp.GetRequiredService<IOptions<JsonSerializerOptions>>();
            var configDir = FileSystem.Current.AppDataDirectory;
            
            return new ConfigHandler(crypt, secureStorage, jsonOptions, configDir);
        });
    }

    public static void AddApiClients(this IServiceCollection services)
    {
        services.AddSingleton<RestClient>();
        services.AddSingleton<LobbyClient>();
        services.AddSingleton<GameClient>();
    }

    public static void ConfigureJsonOptions(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.Converters.Add(new MauiColorJsonConverter());
        });
    }

    public static void ConfigureHandlers(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler(typeof(Microsoft.Maui.Controls.Maps.Map), typeof(AdvancedMapHandler));
        });
    }
}