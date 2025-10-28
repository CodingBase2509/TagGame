using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;

namespace TagGame.Client.Extensions;

/// <summary>
/// IConfigurationBuilder extensions to load appsettings files from the app package (Resources/Raw).
/// </summary>
public static class ConfigurationAppSettings
{
#if DEBUG
    private const string Env = ".Development";
#else
    private const string Env = "";
#endif

    /// <summary>
    /// Loads the environment specific appsettings file from the package and adds it to the builder.
    /// </summary>
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
