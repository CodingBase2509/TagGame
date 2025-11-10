using Microsoft.Extensions.Configuration;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Security;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Extensions;

public static class ServiceCollectionServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IAuthService, AuthService>();
        services.AddOptions<AuthServiceOptions>()
            .Bind(config.GetSection("Auth"));

        services.AddSingleton<ICrypto, Crypto>();

        return services;
    }
}
