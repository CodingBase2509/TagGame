using System.Text.Json;
using Microsoft.Maui.Graphics;
using TagGame.Client.Common;

namespace TagGame.Client.Tests.Unit.Common;

public class MauiColorJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public MauiColorJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new MauiColorJsonConverter());
    }

    [Fact]
    public void Serialize_Color_ReturnsHexString()
    {
        // Arrange
        var color = Color.FromArgb("#FF5733");

        // Act
        var json = JsonSerializer.Serialize(color, _options);

        // Assert
        json.Should().Be("\"#FFFF5733\"");
    }

    [Fact]
    public void Deserialize_HexString_ReturnsCorrectColor()
    {
        // Arrange
        var json = "\"#1E90FF\"";

        // Act
        var color = JsonSerializer.Deserialize<Color>(json, _options);

        // Assert
        color.ToHex().Should().Be("#1E90FF");
    }

    [Fact]
    public void Serialize_TransparentColor_ReturnsHexWithAlpha()
    {
        // Arrange
        var color = Color.FromRgba(255, 255, 255, 0); // Fully transparent white

        // Act
        var json = JsonSerializer.Serialize(color, _options);

        // Assert
        json.Should().Be("\"#00FFFFFF\"");
    }

    [Fact]
    public void Deserialize_InvalidHex_ReturnsNull()
    {
        // Arrange
        var json = "\"not-a-color\"";

        // Act
        var result =  JsonSerializer.Deserialize<Color>(json, _options);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        // Arrange
        var json = "null";

        // Act
        var result = JsonSerializer.Deserialize<Color>(json, _options);

        // Assert
        result.Should().BeNull();
    }
}
