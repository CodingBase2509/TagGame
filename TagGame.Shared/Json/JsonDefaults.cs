using System.Text.Json;
using System.Text.Json.Serialization;

namespace TagGame.Shared.Json;

/// <summary>
/// Central JSON defaults for server and client. Keep framework-agnostic.
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

    private sealed class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.String
                ? ParseString(reader.GetString())
                : reader.TokenType == JsonTokenType.Number
                    ? ParseNumber(ref reader)
                    : throw new JsonException("Unsupported TimeSpan JSON token.");

        private static TimeSpan ParseString(string? s) =>
            string.IsNullOrEmpty(s)
                ? default
                : TimeSpan.TryParse(s, out var ts)
                    ? ts
                    : throw new JsonException($"Invalid TimeSpan value '{s}'.");

        private static TimeSpan ParseNumber(ref Utf8JsonReader reader) =>
            reader.TryGetInt64(out var ms)
                ? TimeSpan.FromMilliseconds(ms)
                : reader.TryGetDouble(out var dms)
                    ? TimeSpan.FromMilliseconds(dms)
                    : throw new JsonException("Invalid numeric TimeSpan value.");

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
            // Use constant (invariant) format, e.g., "hh:mm:ss.fffffff"
            writer.WriteStringValue(value.ToString("c"));
    }
}
