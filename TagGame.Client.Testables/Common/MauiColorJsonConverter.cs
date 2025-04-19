using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;

namespace TagGame.Client.Common;

public class MauiColorJsonConverter : JsonConverter<Color>
{
    public override Color? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var argb = reader.GetString();
        Color.TryParse(argb, out var result);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        var argbString = value.ToArgbHex(includeAlpha: true);
        writer.WriteStringValue(argbString);
                
    }
}