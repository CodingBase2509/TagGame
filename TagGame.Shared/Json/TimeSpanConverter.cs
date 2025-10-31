using System.Text.Json;
using System.Text.Json.Serialization;

namespace TagGame.Shared.Json;

/// <summary>
/// Simple TimeSpan converter that reads ISO8601 constant ("c") or numeric milliseconds and writes constant format.
/// </summary>
internal sealed class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
            ? ParseString(reader.GetString())
            : reader.TokenType == JsonTokenType.Number
                ? ParseNumber(ref reader)
                : throw new JsonException("Unsupported TimeSpan JSON token.");

    private static TimeSpan ParseString(string? s) =>
        string.IsNullOrEmpty(s)
            ? TimeSpan.Zero
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
