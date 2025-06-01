using CoreGraphics;
using MapKit;
using TagGame.Client.Ui.Components;
using Point = Microsoft.Maui.Graphics.Point;

namespace TagGame.Client.Handlers;

public partial class AdvancedMapHandler
{
    public partial Location? ConvertScreenToLocation(Point screenPoint, View source)
    {
        if (PlatformView is not MKMapView mapView)
            return null;

        var cgPoint = new CGPoint(screenPoint.X, screenPoint.Y);
        var coord = mapView.ConvertPoint(cgPoint, mapView);
        return new Location(coord.Latitude, coord.Longitude);
    }
    
    private partial void UpdateRotationEnabled()
    {
        if (PlatformView is not MKMapView mapView || VirtualView is not AdvancedMap advancedMap)
            return;
        
        mapView.RotateEnabled = advancedMap.IsRotationEnabled;
    }
}