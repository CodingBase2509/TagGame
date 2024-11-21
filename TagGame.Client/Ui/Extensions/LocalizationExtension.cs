using TagGame.Client.Services;

namespace TagGame.Client.Ui.Extensions;

public class LocalizationExtension() : IMarkupExtension<string>
{
    /// <summary>
    /// The Key from the localization resource.
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    public string Page { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return $"[Key not set]";
        
        return Localization.Get(Key, Page);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}