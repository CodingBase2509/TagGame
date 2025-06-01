using Microsoft.Maui.Controls;

namespace TagGame.Client.Ui.Components;

public class AdvancedMap : Microsoft.Maui.Controls.Maps.Map
{
    public static readonly BindableProperty IsRotationEnabledProperty =
        BindableProperty.Create(
            nameof(IsRotationEnabled), 
            typeof(bool), 
            typeof(AdvancedMap), 
            true);

    public bool IsRotationEnabled
    {
        get => (bool)GetValue(IsRotationEnabledProperty);
        set => SetValue(IsRotationEnabledProperty, value);
    }
}