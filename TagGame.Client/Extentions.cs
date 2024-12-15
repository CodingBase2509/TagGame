using TagGame.Shared.DTOs.Common;

namespace TagGame.Client;

public static class Extentions
{
    public static ColorDTO ToColorDTO(this Color mauiColor)
    {
        int red = (int)(mauiColor.Red * 255);
        int green = (int)(mauiColor.Green * 255);
        int blue = (int)(mauiColor.Blue * 255);
        int alpha = (int)(mauiColor.Alpha * 255);
        
        return ColorDTO.FromArgb(red, green, blue, alpha);
    }

    public static Color ToMauiColor(this ColorDTO systemColor)
    {
        float red = systemColor.R / 255f;
        float green = systemColor.G / 255f;
        float blue = systemColor.B / 255f;
        float alpha = systemColor.A / 255f;

        return new Color(red, green, blue, alpha);
    }
}