using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Infrastructure.Notifications;

public class ToastPublisher : IToastPublisher, IToastSender
{
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public event EventHandler<ToastRequest>? ToastRequested;

    public async Task PublishAsync(ToastRequest request, CancellationToken ct = default)
    {
        await Lock.WaitAsync(ct);
        ToastRequested?.Invoke(this, request);
        Lock.Release();
    }
}
