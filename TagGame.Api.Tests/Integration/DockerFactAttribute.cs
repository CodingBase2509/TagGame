using Xunit;

namespace TagGame.Api.Tests.Integration;

/// <summary>
/// Fact attribute that skips the test when Docker is not available (needed for Testcontainers).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerRequirement.IsAvailable)
        {
            Skip = "Docker is not available on this machine.";
        }
    }
}
