using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using TagGame.Client.Ui.Components;
using Point = Microsoft.Maui.Graphics.Point;

namespace TagGame.Client.Handlers;

public partial class AdvancedMapHandler
{
    public partial Location? ConvertScreenToLocation(Point screenPoint, View source)
    {
        if (PlatformView is not Android.Views.View || Map is null)
            return null;

        var androidPoint = new Android.Graphics.Point((int)screenPoint.X, (int)screenPoint.Y);
        var latLng = Map.Projection?.FromScreenLocation(androidPoint);
        return latLng is null ? null : new Location(latLng.Latitude, latLng.Longitude);
    }

    private partial void UpdateRotationEnabled()
    {
        if (VirtualView is not AdvancedMap advancedMap || Map is null)
            return;
        Map.UiSettings.RotateGesturesEnabled = advancedMap.IsRotationEnabled;
    }
}