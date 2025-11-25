using ZXing.Net.Maui.Controls;

namespace TagGame.Client.Extensions;

public static class BuilderResources
{
    public static MauiAppBuilder ConfigureBuilder(this MauiAppBuilder builder)
    {
        builder.UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Manrope-ExtraLight.ttf", "ManropeExtraLight");
                fonts.AddFont("Manrope-Light.ttf", "ManropeLight");
                fonts.AddFont("Manrope-Medium.ttf", "ManropeMedium");
                fonts.AddFont("Manrope-Regular.ttf", "ManropeRegular");
                fonts.AddFont("Manrope-SemiBold.ttf", "ManropeSemiBold");
                fonts.AddFont("Manrope-Bold.ttf", "ManropeBold");
                fonts.AddFont("Manrope-ExtraBold.ttf", "ManropeExtraBold");
            });

        return builder;
    }
}
