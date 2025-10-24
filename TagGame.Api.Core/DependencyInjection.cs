using TagGame.Api.Core.Persistence.Contexts;
namespace TagGame.Api.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContexts(configuration);

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        _ = services.AddDbContext<GamesDbContext>(builder => builder.UseNpgsql(connectionString))
            .AddDbContext<AuthDbContext>(builder => builder.UseNpgsql(connectionString));

        return services;
    }
}
