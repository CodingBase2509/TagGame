using Microsoft.AspNetCore.SignalR.Client;

namespace TagGame.Client.Clients;

public interface IHubConnection : IAsyncDisposable
{
    /// <inheritdoc cref="HubConnection.State"/>
    HubConnectionState State { get; }
    /// <inheritdoc cref="HubConnection.StartAsync"/>
    Task StartAsync();
    /// <inheritdoc cref="HubConnection.StopAsync"/>
    Task StopAsync();
    /// <inheritdoc cref="HubConnection.InvokeAsync"/>
    Task InvokeAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);
    /// <inheritdoc cref="HubConnection.InvokeAsync"/>
    Task InvokeAsync(string methodName, CancellationToken cancellationToken = default);
    /// <inheritdoc cref="HubConnection.On"/>
    IDisposable On<T1>(string methodName, Func<T1, Task> handler);
}

public class HubConnectionWrapper(HubConnection inner) : IHubConnection
{
    public HubConnectionState State => inner.State;
    public Task StartAsync() => inner.StartAsync();
    public Task StopAsync() => inner.StopAsync();
    
    public Task InvokeAsync(string methodName, object? arg1, CancellationToken cancellationToken = default)
        => inner.InvokeAsync(methodName, arg1, cancellationToken);

    public Task InvokeAsync(string methodName, CancellationToken cancellationToken = default)
        => inner.InvokeAsync(methodName, cancellationToken);
    
    public IDisposable On<T1>(string methodName, Func<T1, Task> handler)
        => inner.On(methodName, handler);

    public async ValueTask DisposeAsync()
    {
        if (inner is not null) 
            await inner.DisposeAsync();
        
        GC.SuppressFinalize(this);
    }
}