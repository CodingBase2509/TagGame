using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Shared.Json;

namespace TagGame.Client.Core.Json;

/// <summary>
/// Provides a single, shared System.Text.Json options instance for the client.
/// Uses the server-aligned defaults from TagGame.Shared.Json.JsonDefaults.
/// </summary>
public interface IJsonOptionsProvider
{
    JsonSerializerOptions Options { get; }
}

/// <summary>
/// Default implementation that exposes the shared JsonDefaults.Options instance.
/// </summary>
internal sealed class DefaultJsonOptionsProvider : IJsonOptionsProvider
{
    public JsonSerializerOptions Options { get; } = JsonDefaults.Options;
}
