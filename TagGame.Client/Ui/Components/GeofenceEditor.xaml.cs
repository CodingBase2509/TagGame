using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
#if IOS
using CoreGraphics;
#endif
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using TagGame.Client.Handlers;

namespace TagGame.Client.Ui.Components;

public partial class GeofenceEditor : ContentView, IDisposable
{
    private Polygon? _geofencePolygon;
    private Point _panStartCenter;
    private double _currentScale = 1;
    private double _startScale = 1;
    private bool _isRotating = false;
    
    public static readonly BindableProperty InitialGeofenceProperty = BindableProperty.Create(
        nameof(InitialGeofence),
        typeof(Shared.Domain.Common.Location[]),
        typeof(GeofenceEditor),
        Array.Empty<TagGame.Shared.Domain.Common.Location>());
    
    public static readonly BindableProperty GeofenceChangedCommandProperty = BindableProperty.Create(
        nameof(GeofenceChangedCommand),
        typeof(ICommand),
        typeof(GeofenceEditor));
    
    public GeofenceEditor()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public Shared.Domain.Common.Location[] InitialGeofence
    {
        get => (Shared.Domain.Common.Location[])GetValue(InitialGeofenceProperty);
        set => SetValue(InitialGeofenceProperty, value);
    }
    
    public ICommand GeofenceChangedCommand
    {
        get => (ICommand)GetValue(GeofenceChangedCommandProperty);
        set => SetValue(GeofenceChangedCommandProperty, value);
    }
    
    public void Dispose()
    {
        Loaded -= OnLoaded;
        if (_geofencePolygon is not null)
        {
            GameMap.MapElements.Remove(_geofencePolygon);
            _geofencePolygon = null;
        }
        
        GC.SuppressFinalize(this);
    }
    
    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (InitialGeofence is [])
        {
            var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync();
            if (location is null) 
                return;
            
            var mapCenter = new Location(location.Latitude, location.Longitude);
            var radius = Distance.FromMeters(500);
            GameMap.MoveToRegion(MapSpan.FromCenterAndRadius(mapCenter, radius));
        }

        if (_geofencePolygon is null)
        {
            _geofencePolygon = CreatePolygon();
            foreach (var location in InitialGeofence)
            {
                _geofencePolygon.Geopath.Add(new Location(location.Latitude, location.Longitude));
            }
        }
        if (!GameMap.MapElements.Contains(_geofencePolygon))
            GameMap.MapElements.Add(_geofencePolygon);
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

        if (_geofencePolygon?.Count > 0)
        {
            GameMap.MapElements.Remove(_geofencePolygon);
            _geofencePolygon = CreatePolygon();
            GameMap.MapElements.Add(_geofencePolygon);
        }
        foreach (var screenPoint in corners)
        {
            var location = (GameMap.Handler as AdvancedMapHandler)?
                .ConvertScreenToLocation(screenPoint, Overlay);

            if (location is not null)
                _geofencePolygon!.Geopath.Add(location);
        }
        
        GeofenceChangedCommand?.Execute(_geofencePolygon!.Geopath);
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

    private static Polygon CreatePolygon()
    {
        return new Polygon
        {
            StrokeColor = Colors.OrangeRed,
            StrokeWidth = 3,
            FillColor = new Color(1, 0.6f, 0f, 0.2f)
        };
    }
}