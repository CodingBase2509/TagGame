using CoreGraphics;
using MapKit;
using Microsoft.Maui.Maps.Platform;
using TagGame.Client.Ui.Components;
using UIKit;
using Point = Microsoft.Maui.Graphics.Point;

namespace TagGame.Client.Handlers;

public partial class AdvancedMapHandler
{
    protected override void ConnectHandler(MauiMKMapView platformView)
    {
        base.ConnectHandler(platformView);
        
        platformView.OverlayRenderer = (mapView, overlay) =>
        {
            if (overlay is not MKPolygon polygon) 
                return new MKOverlayRenderer(overlay);

            var renderer = new MKPolygonRenderer(polygon)
            {
                FillColor = UIColor.FromRGBA(255, 140, 0, 51), // rgba(255, 140, 0, 0.2)
                StrokeColor = UIColor.Orange,
                LineWidth = 3
            };
            return renderer;
        };
    }

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