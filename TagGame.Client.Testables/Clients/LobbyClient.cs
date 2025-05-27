using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using TagGame.Client.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Client.Clients;

public class LobbyClient(ConfigHandler config) : IAsyncDisposable
{
    private IHubConnection? _connection;

    public async Task InitializeAsync()
    {
        try
        {
            var serverConfig = await config.ReadAsync<ServerConfig>();
            var userConfig = await config.ReadAsync<UserConfig>();
            if (serverConfig is null || userConfig is null)
                return;

            var idString = userConfig?.UserId.ToString() ?? string.Empty;
            var b64Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(idString));
            
            var baseUri = new Uri(serverConfig.Port is not null ? $"{serverConfig.Host}:{serverConfig.Port}" : $"{serverConfig.Host}");
            var hubUri = new Uri(baseUri, ApiRoutes.ILobbyHub.Endpoint);
            var hubCon = new HubConnectionBuilder()
                .WithUrl(hubUri, options =>
                {
                    options.Headers.Add("Authorization", $"Basic {b64Id}");
                })
                .WithStatefulReconnect()
                .WithKeepAliveInterval(TimeSpan.FromSeconds(5))
                .Build();
            
            _connection = new HubConnectionWrapper(hubCon);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task ConnectAsync()
    {
        if (_connection is null)
            return;

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;
        
        try
        {
            await SendDisconnectInfoAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            await _connection.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
    private async Task SendDisconnectInfoAsync()
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            return;

        await _connection.InvokeAsync(nameof(ApiRoutes.ILobbyHub.ReceiveDisconnectInfo));
    }

    // Setup / Receive Methods
    public void SetupReceiveGameRoomInfo(Func<GameRoom, Task> fn)
    {
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceiveGameRoomInfo), fn);
    }

    public void SetupReceivePlayerJoined(Func<Player, Task> fn)
    {
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceivePlayerJoined), fn);
    }

    public void SetupReceivePlayerLeft(Func<PlayerLeftGameInfo, Task> fn)
    {
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceivePlayerLeft), fn);
    }

    public void SetupReceiveGameSettingsUpdated(Func<GameSettings, Task> fn)
    {
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceiveGameSettingsUpdated), fn);
    }

    public void SetupReceiveNewRoomOwner(Func<Guid, Task> fn)
    {
        _connection?.On(nameof(ApiRoutes.ILobbyClient.ReceiveNewRoomOwner), fn);
    }
    
    public void SetupStartGame(Func<int, Task> fn)
    {
        _connection?.On(nameof(ApiRoutes.ILobbyClient.StartCountdown), fn);
    }

    // Send Methods
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