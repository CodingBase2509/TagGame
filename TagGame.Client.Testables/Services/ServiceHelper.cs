using Microsoft.Extensions.DependencyInjection;

namespace TagGame.Client.Services;

public static class ServiceHelper
{
    private static IServiceProvider _serviceProvider;

    public static void SetProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static T? GetService<T>() where T : notnull
    {
        return _serviceProvider.GetService<T>();
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}