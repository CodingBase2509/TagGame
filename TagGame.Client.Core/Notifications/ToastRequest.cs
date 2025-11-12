namespace TagGame.Client.Core.Notifications;

public readonly record struct ToastRequest(
    ToastType Type,
    string Message,
    int DurationMs = 3000,
    bool IsLocalized = true,
    ToastPriority Priority = ToastPriority.Normal);
