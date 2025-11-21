using Docker.DotNet;

namespace TagGame.Api.Tests.Integration;

internal static class DockerRequirement
{
    private static readonly Lazy<bool> Availability = new(IsDockerAvailable);

    public static bool IsAvailable => Availability.Value;

    private static bool IsDockerAvailable()
    {
        try
        {
            var configuration = new DockerClientConfiguration();
            using var client = configuration.CreateClient();
            client.System.PingAsync().GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
