using CommunityToolkit.Maui.Behaviors;
using TagGame.Client.Ui.ToastMessages;

namespace TagGame.Client.Ui.Components;

public partial class ToastView : ContentView
{
    private const string ProgressAnimationName = "ToastDuration";
    
    public ToastView()
    {
        InitializeComponent();
    }
    
    public async Task Show(string message, ToastType type, int durationMilliseconds = 3000)
    {
        this.AbortAnimation(ProgressAnimationName);
        
        // TODO: Add Localization for toast messages
        ToastMessage.Text = message;
        ToastIcon.Source = ImageSource.FromFile(type switch
        {
            ToastType.Success => "success",
            ToastType.Error => "error",
            _ => "info",
        });
        
        var toastColor = type switch
        {
            ToastType.Success => Colors.Green,
            ToastType.Error => Colors.Red,
            _ => Colors.Blue,
        };
        
        ProgressLine.BackgroundColor = toastColor;
        ToastIcon.Behaviors.Add(new IconTintColorBehavior()
        {
            TintColor = toastColor 
        });
        
        ProgressLine.AnchorX = 0;
        ProgressLine.ScaleX = 1;
        
        IsVisible = true;
        AnimateProgressBar(durationMilliseconds);
    }

    void AnimateProgressBar(int durationMilliseconds)
    {
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
}