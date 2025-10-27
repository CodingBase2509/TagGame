namespace TagGame.Client.Core.Services.Abstractions;

public interface INetworkConnectivity
{
    bool IsOnline { get; }
    event EventHandler<bool> OnlineChanged;
    Task WaitForOnlineAsync(CancellationToken ct = default);
}
