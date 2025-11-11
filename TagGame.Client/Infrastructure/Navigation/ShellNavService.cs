using TagGame.Client.Core.Navigation;

namespace TagGame.Client.Infrastructure.Navigation;

public class ShellNavService : INavigationService
{
    private readonly Shell _shell = Shell.Current;

    public async Task GoToAsync(string route, IReadOnlyDictionary<string, object?>? parameters = null, CancellationToken ct = default)
    {
        EnsureShell(ct);
        var qp = ToQueryParameters(parameters);
        await InvokeOnMainThreadAsync(() => _shell.GoToAsync(route, qp));
    }

    public Task GoBackAsync(CancellationToken ct = default)
    {
        EnsureShell(ct);
        return InvokeOnMainThreadAsync(() => _shell.GoToAsync(".."));
    }

    public async Task OpenModalAsync(string route, IReadOnlyDictionary<string, object?>? parameters = null, CancellationToken ct = default)
    {
        EnsureShell(ct);
        var qp = ToQueryParameters(parameters);
        qp["useModalNavigation"] = true;
        await InvokeOnMainThreadAsync(() => _shell.GoToAsync(route, qp));
    }

    public Task CloseModalAsync(CancellationToken ct = default)
    {
        EnsureShell(ct);
        return InvokeOnMainThreadAsync(() => _shell.GoToAsync(".."));
    }

    private static ShellNavigationQueryParameters ToQueryParameters(IReadOnlyDictionary<string, object?>? map)
    {
        var qp = new ShellNavigationQueryParameters();
        if (map is null)
            return qp;

        foreach (var kv in map)
        {
            if (kv.Value is null)
                continue;

            qp[kv.Key] = kv.Value;
        }

        return qp;
    }

    private static void EnsureShell(CancellationToken ct = default)
    {
        if (Shell.Current is null)
            throw new InvalidOperationException("Shell is not initialized.");

        ct.ThrowIfCancellationRequested();
    }

    private static Task InvokeOnMainThreadAsync(Func<Task> func) =>
        MainThread.IsMainThread ? func() : MainThread.InvokeOnMainThreadAsync(func);
}
