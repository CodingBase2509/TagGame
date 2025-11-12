using TagGame.Client.Core.Notifications;
using TagGame.Client.Ui.Components.Toasts;

namespace TagGame.Client.Infrastructure.Notifications;

/// <summary>
/// Bridges toast publish events to the UI host. The host handles sequencing and preemption.
/// </summary>
public sealed class ToastPresenter : IDisposable
{
    private readonly IToastSender _publisher;
    private readonly ToastHost _host;

    public ToastPresenter(IToastSender publisher, ToastHost host)
    {
        _publisher = publisher;
        _host = host;
        _publisher.ToastRequested += OnToastRequested;
    }

    private void OnToastRequested(object? sender, ToastRequest request) =>
        _ = _host.ShowAsync(request);

    public void Dispose() =>
        _publisher.ToastRequested -= OnToastRequested;
}

