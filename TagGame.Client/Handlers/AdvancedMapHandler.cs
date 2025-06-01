using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps.Handlers;
using TagGame.Client.Ui.Components;
using Point = Microsoft.Maui.Graphics.Point;

namespace TagGame.Client.Handlers;

public partial class AdvancedMapHandler() : MapHandler(AdvancedMapper)
{
    private static readonly PropertyMapper<AdvancedMap, AdvancedMapHandler> AdvancedMapper = new(MapHandler.Mapper)
    {
        [nameof(AdvancedMap.IsRotationEnabled)] = MapIsRotationEnabled
    };
    
    public partial Location? ConvertScreenToLocation(Point screenPoint, View source);
    
    public static void MapIsRotationEnabled(AdvancedMapHandler handler, AdvancedMap map)
    {
        handler.UpdateRotationEnabled();
    }

    private partial void UpdateRotationEnabled();
}