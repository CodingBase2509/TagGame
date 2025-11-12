namespace TagGame.Client.Core.Notifications;

public static class IToastPublisherExtensions
{
    public static Task Info(
        this IToastPublisher publisher,
        string text,
        bool isLocalized = true,
        int durationMs = 3000,
        ToastPriority priority = ToastPriority.Normal) =>
        publisher.PublishAsync(new ToastRequest(ToastType.Info, text, durationMs, isLocalized, priority));

    public static Task Success(
        this IToastPublisher publisher,
        string text,
        bool isLocalized = true,
        int durationMs = 3000,
        ToastPriority priority = ToastPriority.Normal) =>
        publisher.PublishAsync(new ToastRequest(ToastType.Success, text, durationMs, isLocalized, priority));

    public static Task Warning(
        this IToastPublisher publisher,
        string text,
        bool isLocalized = true,
        int durationMs = 3000,
        ToastPriority priority = ToastPriority.Normal) =>
        publisher.PublishAsync(new ToastRequest(ToastType.Warning, text, durationMs, isLocalized, priority));

    public static Task Error(
        this IToastPublisher publisher,
        string text,
        bool isLocalized = true,
        int durationMs = 3000,
        ToastPriority priority = ToastPriority.Normal) =>
        publisher.PublishAsync(new ToastRequest(ToastType.Error, text, durationMs, isLocalized, priority));
}
