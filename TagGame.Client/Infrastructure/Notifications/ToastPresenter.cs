using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Ui.Components.Toasts;

namespace TagGame.Client.Infrastructure.Notifications;

/// <summary>
/// Bridges toast publish events to the UI host. The host handles sequencing and preemption.
/// </summary>
public sealed class ToastPresenter : IDisposable
{
    private readonly IToastSender _publisher;

    public ToastHost ToastHost { get; }

    public ToastPresenter(IToastSender publisher, ILocalizer localizer)
    {
        ToastHost = new ToastHost(localizer);
        _publisher = publisher;
        _publisher.ToastRequested += OnToastRequested;
    }

    private void OnToastRequested(object? sender, ToastRequest request) =>
        _ = ToastHost?.ShowAsync(request);

    public void Dispose() => _publisher.ToastRequested -= OnToastRequested;
}

