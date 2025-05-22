using CommunityToolkit.Maui.Behaviors;
using TagGame.Client.Services;
using TagGame.Client.Ui.ToastMessages;

namespace TagGame.Client.Ui.Components;

public partial class ToastView : ContentView
{
    private const string ProgressAnimationName = "ToastDuration";
    private const string LocalizationKey = "ToastMessages";
    
    public ToastView()
    {
        InitializeComponent();
    }
    
    public async Task Show(string message, ToastType type, int durationMilliseconds = 3000)
    {
        this.AbortAnimation(ProgressAnimationName);
        SetText(message);   
        SetIcon(type);
        
        SetStyle(type);
        
        AnimateProgressBar(durationMilliseconds);
    }

    private void SetText(string message)
    {
        var loc = ServiceHelper.GetRequiredService<Localization>();
        ToastMessage.Text = loc.Get(message, LocalizationKey);
    }
    
    private void SetIcon(ToastType type)
    {
        ToastIcon.Source = ImageSource.FromFile(type switch
        {
            ToastType.Success => "success",
            ToastType.Error => "error",
            _ => "info",
        });
    }

    private void SetStyle(ToastType type)
    {
        var style = Style.FromToastType(type);
        
        ProgressLine.BackgroundColor = style.Event;
        ToastIcon.Behaviors.Clear();
        ToastIcon.Behaviors.Add(new IconTintColorBehavior()
        {
            TintColor = style.Event
        });

        ToastBorder.Background = style.Background;

        ToastMessage.TextColor = style.Text;
    }
    
    private void AnimateProgressBar(int durationMilliseconds)
    {
        ProgressLine.AnchorX = 0;
        ProgressLine.ScaleX = 1;
        
        IsVisible = true;
        
        var animation = new Animation(
            x => ProgressLine.ScaleX = x, 
            1, 
            0);
        
        animation.Commit(
            this, 
            ProgressAnimationName, 
            16, 
            (uint)durationMilliseconds, 
            Easing.Linear,
            (v, c) => IsVisible = false);
    }
    
    private class Style
    {
        public Color Background { get; set; }
        public Color Text { get; set; }
        public Color Event { get; set; }

        public static Style FromToastType(ToastType type)
        {
            return type switch
            {
                ToastType.Error => new Style
                {
                    Background = Color.FromArgb("#FFDDDD"), 
                    Text = Color.FromArgb("#7A0000"), 
                    Event = Color.FromArgb("#D32F2F")
                },
                ToastType.Success => new  Style
                {
                    Background = Color.FromArgb("#DDF5E3"), 
                    Text = Color.FromArgb("#1B5E20"), 
                    Event = Color.FromArgb("#388E3C")
                },
                _ => new Style 
                { 
                    Background = Color.FromArgb("#DDEBFF"), 
                    Text = Color.FromArgb("#0D47A1"), 
                    Event = Color.FromArgb("#1976D2")
                }
            };
        }
    }
}