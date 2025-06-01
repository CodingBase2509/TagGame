using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
#if IOS
using CoreGraphics;
#endif
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using TagGame.Client.Handlers;

namespace TagGame.Client.Ui.Components;

public partial class GeofenceEditor : ContentView
{
    private Point _panStartCenter;
    private double _currentScale = 1;
    private double _startScale = 1;
    private bool _isRotating = false;
    
    public static BindableProperty GeofenceChangedProperty = BindableProperty.Create(
        nameof(GeofenceChanged),
        typeof(ICommand),
        typeof(GeofenceEditor));
    
    public GeofenceEditor()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    
    ~GeofenceEditor()
    {
        Loaded -= OnLoaded;
    }

    public ICommand GeofenceChanged
    {
        get => (ICommand)GetValue(GeofenceChangedProperty);
        set => SetValue(GeofenceChangedProperty, value);
    }
    
    private async void OnLoaded(object? sender, EventArgs e)
    {
        var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync();
        if (location is null) 
            return;
        
        var center = new Location(location.Latitude, location.Longitude);
        GameMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(100)));
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = GeofenceBox.Scale;
                break;
            case GestureStatus.Running:
                _currentScale = Math.Clamp(_startScale * e.Scale, 0.5, 5.0);
                GeofenceBox.Scale = _currentScale;
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
            default:
                break;
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        var absLayoutSize = Overlay.Bounds;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
            {
                var bounds = AbsoluteLayout.GetLayoutBounds(GeofenceBox);
                _panStartCenter = new Point(bounds.X, bounds.Y);
                break;
            }
            case GestureStatus.Running:
            {
                double deltaX = e.TotalX / absLayoutSize.Width * 1.6;
                double deltaY = e.TotalY / absLayoutSize.Height * 1.6;

                double newX = Math.Clamp(_panStartCenter.X + deltaX, 0.0, 1.0);
                double newY = Math.Clamp(_panStartCenter.Y + deltaY, 0.0, 1.0);

                AbsoluteLayout.SetLayoutBounds(GeofenceBox,
                    new Rect(newX, newY, GeofenceBox.Width, GeofenceBox.Height));
                break;
            }
            case GestureStatus.Completed:
                DrawBoxViewAsPolygon();
                break;
            case GestureStatus.Canceled:
            default:
                break;
        }
    }



    private void OnTouchAction(object sender, LongPressCompletedEventArgs e)
    {

    }

    private void DrawBoxViewAsPolygon()
    {
        var corners = GetBoxViewCorners();
        var polygon = new Polygon
        {
            StrokeColor = Colors.OrangeRed,
            StrokeWidth = 3,
            FillColor = new Color(1, 0.6f, 0f, 0.2f)
        };

        foreach (var screenPoint in corners)
        {
            var location = (GameMap.Handler as AdvancedMapHandler)?
                .ConvertScreenToLocation(screenPoint, Overlay);

            if (location is not null)
                polygon.Geopath.Add(location);
        }

        GameMap.MapElements.Clear();
        GameMap.MapElements.Add(polygon);
        
        GeofenceChanged?.Execute(polygon);
    }

    private List<Point> GetBoxViewCorners()
    {
#if ANDROID
        if (GeofenceBox.Handler?.PlatformView is not Android.Views.View boxNative ||
            GameMap.Handler?.PlatformView is not Android.Views.View mapNative)
            return [];

        var boxPos = new int[2];
        var mapPos = new int[2];

        boxNative.GetLocationOnScreen(boxPos);
        mapNative.GetLocationOnScreen(mapPos);

        double relativeX = boxPos[0] - mapPos[0];
        double relativeY = boxPos[1] - mapPos[1];

        double width = boxNative.MeasuredWidth * GeofenceBox.Scale;
        double height = boxNative.MeasuredHeight * GeofenceBox.Scale;
        double centerX = relativeX + width / 2;
        double centerY = relativeY + height / 2;
#endif

#if IOS
        if (GeofenceBox.Handler?.PlatformView is not UIKit.UIView boxNative || 
            Overlay.Handler?.PlatformView is not UIKit.UIView overlayNative)
            return [];

        var boxOrigin = boxNative.ConvertPointToView(CGPoint.Empty, overlayNative);

        // Breite & Höhe im UI-Koordinatensystem
        double width = boxNative.Frame.Width * GeofenceBox.Scale;
        double height = boxNative.Frame.Height * GeofenceBox.Scale;
        double centerX = boxOrigin.X + width / 2;
        double centerY = boxOrigin.Y + height / 2;
#endif

        double angle = GeofenceBox.Rotation * Math.PI / 180;
        var corners = new List<Point>();

        foreach (var (dx, dy) in new[] { (-0.5, -0.5), (0.5, -0.5), (0.5, 0.5), (-0.5, 0.5) })
        {
            double localX = dx * width;
            double localY = dy * height;
            double rotX = localX * Math.Cos(angle) - localY * Math.Sin(angle);
            double rotY = localX * Math.Sin(angle) + localY * Math.Cos(angle);
            corners.Add(new Point(centerX + rotX, centerY + rotY));
        }

        return corners;
    }
    
    
}