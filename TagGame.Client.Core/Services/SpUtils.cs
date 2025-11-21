namespace TagGame.Client.Core.Services;

public static class SpUtils
{
    public static IServiceProvider? Services { get; private set; }

    public static void Set(IServiceProvider serviceProvider) => Services = serviceProvider;

    public static T? GetService<T>() where T : class => Services?.GetService<T>();

    public static T GetRequiredService<T>() where T : class => Services!.GetRequiredService<T>();
}
