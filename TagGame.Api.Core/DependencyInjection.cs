using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
namespace TagGame.Api.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContexts(configuration);
        services.AddAuthenticationServices();
        services.AddSingleton(TimeProvider.System);

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
