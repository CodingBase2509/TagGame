using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using TagGame.Shared.DTOs.Common;

namespace TagGame.Client.Ui.Extensions;

public class ColorMauiDTOConverter : IValueConverter
{
    // DTO to Maui-Color
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as ColorDTO)?.ToMauiColor();
    }

    // Maui Color to DTO
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as Color)?.ToColorDTO();
    }
}