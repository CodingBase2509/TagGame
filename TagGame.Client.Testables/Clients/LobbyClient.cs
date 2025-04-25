using Microsoft.AspNetCore.SignalR.Client;
using TagGame.Client.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Client.Clients;

public class LobbyClient(ConfigHandler config) : IAsyncDisposable
{
    private HubConnection? _connection;
    
    public async Task InitializeAsync()
    {
        var serverConfig = await config.ReadAsync<ServerConfig>();
        if (serverConfig is null)
            return;
        
        _connection = new HubConnectionBuilder()
            .WithUrl(Path.Combine(serverConfig.Host, ApiRoutes.ILobbyHub.Endpoint))
            .WithStatefulReconnect()
            .WithKeepAliveInterval(TimeSpan.FromSeconds(5))
            .Build();

        await StartAsync();
    }

    public async Task StartAsync()
    {
        if (_connection is null)
            return;
        
        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        await _connection.StopAsync();
    }

    public void SetupReceiveGameRoomInfo(Func<GameRoom, Task> fn)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceiveGameRoomInfo), fn);
    }

    public void SetupReceivePlayerJoined(Func<Player, Task> fn)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceivePlayerJoined), fn);
    }

    public void SetupReceivePlayerLeft(Func<PlayerLeftGameInfo, Task> fn)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceivePlayerLeft), fn);
    }

    public void SetupReceiveGameSettingsUpdated(Func<GameSettings, Task> fn)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceiveGameSettingsUpdated), fn);
    }

    public void SetupStartGame(Func<int, Task> fn)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        _connection?.On(nameof(ApiRoutes.ILobbyClient.StartCountdown), fn);
    }

    public async Task UpdateGameSettingsAsync(GameSettings settings)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        await _connection.InvokeAsync(nameof(ApiRoutes.ILobbyHub.UpdateGameSettings), settings);
    }

    public async Task StartGameAsync()
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;

        await _connection.InvokeAsync(nameof(ApiRoutes.ILobbyHub.StartGame));
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) 
            await _connection.DisposeAsync();
        
        GC.SuppressFinalize(this);
    }
}