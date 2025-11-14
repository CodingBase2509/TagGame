using System.Runtime.CompilerServices;

namespace TagGame.Client.Core.Notifications;

public interface IToastPublisher
{
    Task PublishAsync(ToastRequest request, CancellationToken ct = default);
}
