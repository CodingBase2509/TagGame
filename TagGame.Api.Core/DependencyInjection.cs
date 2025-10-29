using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Api.Core.Persistence.Repositories;
namespace TagGame.Api.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddAuthenticationServices();

        services.AddDbContexts(configuration);
        services.AddRepositories();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IDbRepository<>), typeof(EfDbRepository<>));
        services.AddScoped<IAuthUoW, AuthUnitOfWork>();
        services.AddScoped<IGamesUoW, GamesUnitOfWork>();

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<GamesDbContext>(builder => builder.UseNpgsql(connectionString))
            .AddDbContext<AuthDbContext>(builder => builder.UseNpgsql(connectionString));

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>().BindConfiguration("Jwt");
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        return services;
    }
}
