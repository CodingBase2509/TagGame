using Microsoft.Maui.Controls.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.Extensions;

[RequireService([typeof(Localization)])]
public class LocalizationExtension : IMarkupExtension<string>
{
    /// <summary>
    /// The Key from the localization resource.
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    public string Page { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        var loc = ServiceHelper.GetRequiredService<Localization>();
        return string.IsNullOrEmpty(Key) ? $"[Key not set]" : loc.Get(Key, Page);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
