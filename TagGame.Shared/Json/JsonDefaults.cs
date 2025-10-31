using System.Text.Json;
using System.Text.Json.Serialization;

namespace TagGame.Shared.Json;

/// <summary>
/// Central JSON defaults for server and client. Keep framework-agnostic and align both sides.
/// </summary>
public static class JsonDefaults
{
    /// <summary>
    /// A reusable, immutable baseline options instance.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    /// <summary>
    /// Applies the shared defaults to an existing options instance.
    /// </summary>
    public static void Apply(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // Ensure enums are serialized as strings (camelCase). Allow integers on read for tolerance.
        if (!options.Converters.OfType<JsonStringEnumConverter>().Any())
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        // TimeSpan converter for consistent round-trips (c format), if not present.
        if (!options.Converters.Any(c => c is TimeSpanConverter))
        {
            options.Converters.Add(new TimeSpanConverter());
        }
    }

    private static JsonSerializerOptions Create()
    {
        var o = new JsonSerializerOptions();
        Apply(o);
        return o;
    }
}
