using TagGame.Client.Core.Json;

namespace TagGame.Client.Core.Extensions;

public static class ServiceCollectionJsonOptionsExtensions
{
    public static IServiceCollection AddJsonOptionsProvider(this IServiceCollection services) =>
        services.AddSingleton<IJsonOptionsProvider, DefaultJsonOptionsProvider>();
}
