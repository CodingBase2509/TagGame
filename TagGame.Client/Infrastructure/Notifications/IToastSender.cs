using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Infrastructure.Notifications;

public interface IToastSender
{
    event EventHandler<ToastRequest>? ToastRequested;
}
