using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Api.Services;

namespace TagGame.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDbLayer(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<GamesDbContext>(options => 
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDatabase, GamesDbContext>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<GameRoomService>();
        services.AddScoped<PlayerService>();

        return services;
    }
}
