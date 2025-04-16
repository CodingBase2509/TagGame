using System.Text.Json;
using System.Text.Json.Serialization;

namespace TagGame.Shared.Constants;

public static class MappingOptions
{
    public static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        AllowTrailingCommas = true,
    };
}
