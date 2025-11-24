using TagGame.Client.Core.Ui.Services;
using TagGame.Client.Core.Ui.ViewModels.Start;

namespace TagGame.Client.Core.Extensions;

public static class ServiceCollectionViewModels
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<UserInitService>();
        services.AddTransient<UserInitViewModel>();

        services.AddTransient<StartViewModel>();

        return services;
    }
}
