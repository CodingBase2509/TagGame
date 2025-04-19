using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using TagGame.Client.Clients;
using TagGame.Client.Common;
using TagGame.Client.Services;
using TagGame.Client.Ui;
using TagGame.Client.Ui.Extensions;
using TagGame.Client.Ui.ViewModels;
using TagGame.Client.Ui.Views;
using TagGame.Shared.Constants;
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
        
        return services;
    }

    public static IServiceCollection AddApiClients(this IServiceCollection services)
    {
        services.AddSingleton<RestClient>();
        services.AddSingleton<LobbyClient>();
        services.AddSingleton<GameClient>();
        
        return services;
    }

    public static IServiceCollection ConfigureJsonOptions(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.Converters.Add(new MauiColorJsonConverter());
        });

        return services;
    }
}