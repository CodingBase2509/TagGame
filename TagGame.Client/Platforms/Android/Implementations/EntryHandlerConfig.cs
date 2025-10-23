using Android.Graphics.Drawables;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace TagGame.Client;

public static class EntryHandlerConfig
{
    public static MauiAppBuilder CustomizeEntryHandlers(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(_ =>
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(IEntry.Background), (handler, view) =>
            {
                var drawable = new GradientDrawable();
                drawable.SetColor(Android.Graphics.Color.Azure);
                drawable.SetCornerRadius(15f);
                handler.PlatformView.Background = drawable;
                handler.PlatformView.SetHeight(30);
            });
        });
        
        return builder;
    }
}