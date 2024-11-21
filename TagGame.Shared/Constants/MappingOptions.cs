using System.Text.Json;
using System.Text.Json.Serialization;

namespace TagGame.Shared.Constants;

public class MappingOptions
{
    public static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        AllowTrailingCommas = true,
    };
}
