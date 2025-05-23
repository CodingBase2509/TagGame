using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagGame.Client.Ui.Components;

public partial class LabelElement : ContentView
{
    public static readonly BindableProperty TextProperty = 
        BindableProperty.Create(
            nameof(Text),
            typeof(string), 
            typeof(LabelElement),
            string.Empty);
    
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(
            nameof(Orientation),
            typeof(StackOrientation),
            typeof(LabelElement),
            StackOrientation.Vertical,
            propertyChanged: OnOrientationChanged);
    
    public static readonly BindableProperty LabelFontSizeProperty =
        BindableProperty.Create(
            nameof(LabelFontSize),
            typeof(double),
            typeof(LabelElement),
            10.0);
    
    public static readonly BindableProperty LabelSpacingProperty = 
        BindableProperty.Create(
            nameof(LabelSpacing),
            typeof(double),
            typeof(LabelElement),
            5.0); 
    
    public LabelElement()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public double LabelSpacing
    {
        get => (double)GetValue(LabelSpacingProperty);
        set => SetValue(LabelSpacingProperty, value);
    }
    
    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not LabelElement element)
            return;

        element.LabelFontSize = newValue switch
        {
            StackOrientation.Vertical => 10,
            StackOrientation.Horizontal => 16,
            _ => 10,
        };
        
        element.OnPropertyChanged(nameof(element.LabelFontSize));
    }
}