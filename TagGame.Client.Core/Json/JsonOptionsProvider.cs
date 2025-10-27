using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Shared.Json;

namespace TagGame.Client.Core.Json;

public interface IJsonOptionsProvider
{
    JsonSerializerOptions Options { get; }
}

internal sealed class DefaultJsonOptionsProvider : IJsonOptionsProvider
{
    public JsonSerializerOptions Options { get; } = JsonDefaults.Options;
}

