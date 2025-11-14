namespace TagGame.Client.Core.Services;

/// <summary>
/// Abstraction for monitoring network connectivity in a UI-agnostic way.
/// </summary>
public interface INetworkConnectivity
{
    /// <summary>
    /// Indicates whether the device currently has internet connectivity.
    /// </summary>
    bool IsOnline { get; }

    /// <summary>
    /// Raised when connectivity changes. The boolean argument is true when going online, false when going offline.
    /// </summary>
    event EventHandler<bool> OnlineChanged;

    /// <summary>
    /// Returns a task that completes when connectivity is available. If already online, returns a completed task.
    /// </summary>
    Task WaitForOnlineAsync(CancellationToken ct = default);
}
