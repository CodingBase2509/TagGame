using Microsoft.Extensions.Configuration;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;
using TagGame.Client.Core.Services.Implementations;

namespace TagGame.Client.Core.Extensions;

public static class ServiceCollectionServices
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IAuthService, AuthService>();
        services.AddOptions<AuthServiceOptions>()
            .Bind(config.GetSection("Auth"));

        return services;
    }
}
