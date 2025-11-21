using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Core.Services;
using TagGame.Client.Ui.Components.Toasts;

namespace TagGame.Client.Infrastructure.Notifications;

/// <summary>
/// Bridges toast publish events to the UI host. The host handles sequencing and preemption.
/// </summary>
public sealed class ToastPresenter : IDisposable
{
    private readonly IToastSender _publisher;
    private readonly IUiDispatcher _dispatcher;

    public ToastHost ToastHost { get; }

    public ToastPresenter(IToastSender publisher, ILocalizer localizer, IUiDispatcher dispatcher)
    {
        ToastHost = new ToastHost(localizer);
        _publisher = publisher;
        _publisher.ToastRequested += OnToastRequested;
        _dispatcher = dispatcher;
    }

    private void OnToastRequested(object? sender, ToastRequest request) =>
        _ = ToastHost?.ShowAsync(request, _dispatcher);

    public void Dispose() => _publisher.ToastRequested -= OnToastRequested;
}

