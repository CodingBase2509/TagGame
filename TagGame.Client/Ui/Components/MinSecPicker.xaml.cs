using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TagGame.Client.Ui.Components;

public partial class MinSecPicker : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value),
            typeof(TimeSpan),
            typeof(MinSecPicker),
            default(TimeSpan),
            BindingMode.TwoWay,
            propertyChanged: OnValuePropertyChanged);
    
    public MinSecPicker()
    {
        InitializeComponent();
        InitLists();
    }
    
    public TimeSpan Value
    {
        get => (TimeSpan)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    void InitLists()
    {
        for (var m = 0; m < 300; m++)
            MinutesPicker.Items.Add(m.ToString("D2"));
        
        for (var s = 0; s < 60; s++)
            SecondsPicker.Items.Add(s.ToString("D2"));
        
        UpdatePickersFromValue(Value);
    }
    
    private static void OnValuePropertyChanged(BindableObject bindable, object oldVal, object newVal)
    {
        if (bindable is MinSecPicker control && newVal is TimeSpan ts)
            control.UpdatePickersFromValue(ts);
    }
    
    private void OnMinutesChanged(object? sender, EventArgs e) => UpdateValueFromPickers();
    private void OnSecondsChanged(object? sender, EventArgs e) => UpdateValueFromPickers();

    private void UpdatePickersFromValue(TimeSpan ts)
    {
        MinutesPicker.SelectedIndex = ts.Minutes;
        SecondsPicker.SelectedIndex = ts.Seconds;
    }

    private void UpdateValueFromPickers()
    {
        if (MinutesPicker.SelectedIndex < 0 || SecondsPicker.SelectedIndex < 0)
            return;

        Value = new TimeSpan(0,
                             MinutesPicker.SelectedIndex,
                             SecondsPicker.SelectedIndex);
    }

}