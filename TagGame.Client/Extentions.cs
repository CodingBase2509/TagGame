namespace TagGame.Client;

public static class Extentions
{
    public static System.Drawing.Color ToSystemColor(this Microsoft.Maui.Graphics.Color mauiColor)
    {
        int red = (int)(mauiColor.Red * 255);
        int green = (int)(mauiColor.Green * 255);
        int blue = (int)(mauiColor.Blue * 255);
        int alpha = (int)(mauiColor.Alpha * 255);
        
        return System.Drawing.Color.FromArgb(red, green, blue, alpha);
    }

    public static Microsoft.Maui.Graphics.Color ToMauiColor(this System.Drawing.Color systemColor)
    {
        float red = systemColor.R / 255f;
        float green = systemColor.G / 255f;
        float blue = systemColor.B / 255f;
        float alpha = systemColor.A / 255f;

        return new Microsoft.Maui.Graphics.Color(red, green, blue, alpha);
    }
}