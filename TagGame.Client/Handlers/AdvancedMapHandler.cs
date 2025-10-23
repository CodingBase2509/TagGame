using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using TagGame.Client.Ui.Components;
using Point = Microsoft.Maui.Graphics.Point;

namespace TagGame.Client.Handlers;

public partial class AdvancedMapHandler() : MapHandler(AdvancedMapper)
{
    private static readonly PropertyMapper<AdvancedMap, AdvancedMapHandler> AdvancedMapper = new(MapHandler.Mapper)
    {
        [nameof(AdvancedMap.IsRotationEnabled)] = MapIsRotationEnabled,
        [nameof(AdvancedMap.InitialGeofence)] = MapInitialGeofence,
    };
    
    public partial Location? ConvertScreenToLocation(Point screenPoint, View source);
    
    public static void MapIsRotationEnabled(AdvancedMapHandler handler, AdvancedMap map)
    {
        handler.UpdateRotationEnabled();
    }

    public static void MapInitialGeofence(AdvancedMapHandler handler, AdvancedMap map)
    {
        if (map.InitialGeofence.Length == 0)
            return;

        var locations = map.InitialGeofence
            .Select(l => new Location(l.Latitude, l.Longitude));

        var center = GetMapLocationFromGeofence(locations);
        var radius = GetMapRadiusFromGeofence(locations);
        
        map.MoveToRegion(MapSpan.FromCenterAndRadius(center, radius));
    }

    private partial void UpdateRotationEnabled();
    
    private static Location GetMapLocationFromGeofence(IEnumerable<Location> geofence)
    {
        double avgLat = geofence.Average(p => p.Latitude);
        double avgLon = geofence.Average(p => p.Longitude);

        return new Location(avgLat, avgLon);
    }

    private static Distance GetMapRadiusFromGeofence(IEnumerable<Location> geofence)
    {
        double maxDistance = 0;
        var mauiLocation = geofence
            .Select(p => new Location(p.Latitude, p.Longitude))
            .ToArray();
        
        for (int i = 0; i < mauiLocation.Length; i++)
        {
            for (int j = i + 1; j < mauiLocation.Length; j++)
            {
                var dist = Location.CalculateDistance(
                    mauiLocation[i],
                    mauiLocation[j],
                    DistanceUnits.Kilometers);

                if (dist > maxDistance)
                    maxDistance = dist;
            }
        }

        return Distance.FromKilometers(maxDistance);
    }
}