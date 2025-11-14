using Microsoft.Maui.Controls.Shapes;
using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Ui.Components.Toasts;

public partial class ToastHost : Grid
{
    private readonly ILocalizer _loc;
    private readonly List<ToastEntry> _entries = [];
    private ToastEntry? _activeTop;

    public ToastHost(ILocalizer localizer)
    {
        _loc = localizer;
        InitializeComponent();
    }

    public Task ShowAsync(ToastRequest req) =>
        UiUtils.OnMainThreadAsync(() => ShowInternalAsync(req));

    private Task ShowInternalAsync(ToastRequest req)
    {
        var view = CreateToastView(req);
        var entry = new ToastEntry(req, view);

        InsertEntry(entry);
        return entry.Completion;
    }

    private Toast CreateToastView(ToastRequest req)
    {
        var toast = new Toast
        {
            Text = ToastHostService.ResolveText(req, _loc),
            Type = req.Type
        };

        toast.Apply();
        ToastHostService.PrepareToast(toast);
        return toast;
    }

    private void InsertEntry(ToastEntry entry)
    {
        var isPriority = ToastHostService.IsPriority(entry.ToastRequest);
        var insertIndex = isPriority ? 0 : _entries.Count;

        _entries.Insert(insertIndex, entry);
        Children.Add(entry.ToastView);
        UpdateStackVisuals();
        _ = RunEntryLifecycleAsync(entry);
    }

    private void UpdateStackVisuals()
    {
        for (var i = 0; i < _entries.Count; i++)
        {
            var toast = _entries[i].ToastView;
            toast.Margin = i != 0 ? new Thickness(0, 5, 0, 0) : 0;

            toast.ZIndex = _entries.Count - i;
        }

        Badge.IsVisible = _entries.Count > 1;
        if (Badge.IsVisible)
            BadgeLabel.Text = ToastHostService.FormatBadgeCount(_entries.Count - 1);
    }

    private async Task RunEntryLifecycleAsync(ToastEntry entry)
    {
        await ToastHostService.AnimateInAsync(entry.ToastView);
        EnsureTopTimer();

        await entry.Completion;

        await ToastHostService.AnimateOutAsync(entry.ToastView);
        RemoveEntry(entry);
    }

    private void RemoveEntry(ToastEntry entry)
    {
        if (!_entries.Remove(entry))
            return;

        Children.Remove(entry.ToastView);
        entry.Dispose();

        if (ReferenceEquals(_activeTop, entry))
            _activeTop = null;

        UpdateStackVisuals();
        EnsureTopTimer();
    }

    private void EnsureTopTimer()
    {
        var next = _entries.FirstOrDefault();
        if (ReferenceEquals(next, _activeTop))
            return;

        _activeTop?.PauseTimer();
        _activeTop = next;

        if (_activeTop is null)
            return;

        _ = RunTopTimerAsync(_activeTop);
    }

    private async Task RunTopTimerAsync(ToastEntry entry)
    {
        var token = entry.StartTimer();
        if (token == CancellationToken.None)
        {
            entry.Complete();
            return;
        }

        try
        {
            await ToastHostService.WaitDurationAsync(Dispatcher, entry.RemainingMs, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        entry.Complete();
    }
}
