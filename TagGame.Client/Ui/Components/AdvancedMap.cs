using System;
using Microsoft.Maui.Controls;
using Location = TagGame.Shared.Domain.Common.Location;

namespace TagGame.Client.Ui.Components;

public class AdvancedMap : Microsoft.Maui.Controls.Maps.Map
{
    public static readonly BindableProperty IsRotationEnabledProperty =
        BindableProperty.Create(
            nameof(IsRotationEnabled), 
            typeof(bool), 
            typeof(AdvancedMap), 
            true);

    public static readonly BindableProperty InitialGeofenceProperty = 
        BindableProperty.Create(
            nameof(InitialGeofence),
            typeof(Location[]),
            typeof(AdvancedMap),
            defaultValue: Array.Empty<Location>());
    
    public bool IsRotationEnabled
    {
        get => (bool)GetValue(IsRotationEnabledProperty);
        set => SetValue(IsRotationEnabledProperty, value);
    }

    public Location[] InitialGeofence
    {
        get => (Location[])GetValue(InitialGeofenceProperty);
        set => SetValue(InitialGeofenceProperty, value);
    }
}