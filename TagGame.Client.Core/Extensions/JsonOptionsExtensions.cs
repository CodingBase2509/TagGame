using TagGame.Client.Core.Json;

namespace TagGame.Client.Core.Extensions;

public static class JsonOptionsExtensions
{
    public static IServiceCollection AddJsonOptionsProvider(this IServiceCollection services) =>
        services.AddSingleton<IJsonOptionsProvider, DefaultJsonOptionsProvider>();
}
