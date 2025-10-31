using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Filters;
using TagGame.Shared.Json;

namespace TagGame.Api.Extensions;

public static class ServiceCollectionSignalR
{
    public static IServiceCollection AddConfiguredSignalR(this IServiceCollection services)
    {
        services.AddSignalR()
            .AddJsonProtocol(o => JsonDefaults.Apply(o.PayloadSerializerOptions));

        // Global Hub filters
        services.AddSingleton<IHubFilter, RoomAuthHubFilter>();

        return services;
    }
}
