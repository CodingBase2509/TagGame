using Microsoft.Extensions.Configuration;

namespace TagGame.Client.Extensions;

public static class ConfigurationAppSettings
{
#if DEBUG
    private const string Env = ".Development";
#else
    private const string Env = "";
#endif

    public static IConfigurationBuilder AddAppSettingsFile(this IConfigurationBuilder config)
    {
        try
        {
            using var file = FileSystem.OpenAppPackageFileAsync($"appsettings{Env}.json")
                .GetAwaiter()
                .GetResult();

            config.AddJsonStream(file);
        }
        catch
        {
        }

        return config;
    }
}
