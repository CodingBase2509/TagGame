using TagGame.Client.Core.Json;

namespace TagGame.Client.Core.Extensions;

/// <summary>
/// ServiceCollection extensions to register the shared JSON options provider for the client.
/// </summary>
public static class ServiceCollectionJsonOptionsExtensions
{
    /// <summary>
    /// Adds <see cref="IJsonOptionsProvider"/> with shared, server-aligned JSON defaults.
    /// </summary>
    public static IServiceCollection AddJsonOptionsProvider(this IServiceCollection services) =>
        services.AddSingleton<IJsonOptionsProvider, DefaultJsonOptionsProvider>();
}
