using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Common;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Tests.Integration;

public class LobbyHubTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> _factory;

    public LobbyHubTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        UseDbTestContainer();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<GamesDbContext>(options =>
                    options.UseNpgsql(_dbContainer.GetConnectionString()));
            });
        });
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            await context.Database.MigrateAsync();
    }

    private HubConnection CreateHubConnection(string userId)
    {
        var client = _factory.CreateClient();
        
        var b64Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(userId));
        var connection = new HubConnectionBuilder()
            .WithUrl(client.BaseAddress + "lobby", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Basic {b64Id}");
            })
            .Build();

        return connection;
    }

    private async Task<Guid> CreateEntities(bool createPlayer, bool createRoom, bool addPlayerToRoom = false)
    {
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        
        Player? player = null;
        GameRoom? room = null;
        bool success = false;
        
        var user = await userService.AddUserAsync(
            _fixture.Create<string>(), 
            _fixture.Create<ColorDTO>());
        if (createPlayer)
            player = await playerService.CreatePlayerAsync(user.Id);
        if(createRoom)
            room = await roomService.CreateAsync(user.Id, "Test1234");
        
        if (addPlayerToRoom && createRoom && createPlayer)
            success = await playerService.AddPlayerToRoomAsync(room.Id, player.Id);
        
        return user.Id;
    }
    
    [Fact]
    public async Task Connect_ShouldAbort_WhenPlayerNotFound()
    {
        var userId = await CreateEntities(false, true);
        var connection = CreateHubConnection(userId.ToString());
        
        await connection.StartAsync();
        await Task.Delay(500);

        connection.State.Should().Be(HubConnectionState.Disconnected);
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Connect_ShouldAbort_WhenPlayerHasNoRoom()
    {
        var userId = await CreateEntities(true, false);
        var connection = CreateHubConnection(userId.ToString());

        await connection.StartAsync();
        await Task.Delay(500);

        connection.State.Should().Be(HubConnectionState.Disconnected);
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Connect_ShouldJoinGroupAndReceiveInfo_WhenPlayerAndRoomExist()
    {
        var userId = await CreateEntities(true, true, true);
        var connection = CreateHubConnection(userId.ToString());
        
        GameRoom? receivedRoom = null;

        connection.On<GameRoom>("ReceiveGameRoomInfo", room => {
            receivedRoom = room;
            return Task.CompletedTask;
        });

        await connection.StartAsync();
        await Task.Delay(1000);

        receivedRoom.Should().NotBeNull();

        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Disconnect_ShouldRemovePlayerAndNotifyOthers_WhenPlayerLeftGame()
    {
        var userId1 = await CreateEntities(true, true, true);
        var connection1 = CreateHubConnection(userId1.ToString());
        
        var userId2 = await CreateEntities(true, true, true);
        var connection2 = CreateHubConnection(userId1.ToString());

        PlayerLeftGameInfo? leftInfo = null;

        connection1.On<PlayerLeftGameInfo>("ReceivePlayerLeft", info => {
            leftInfo = info;
            return Task.CompletedTask;
        });

        await connection1.StartAsync();
        await connection2.StartAsync();
        await Task.Delay(500);
        
        await connection2.StopAsync();
        await Task.Delay(500);
        
        await connection1.StopAsync();

        leftInfo.Should().NotBeNull();
        leftInfo!.DisconnectType.Should().Be(PlayerDisconnectType.LeftGame);

        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }

    [Fact]
    public async Task Disconnect_ShouldDeleteRoom_WhenLastPlayerLeaves()
    {
        var userId = await CreateEntities(true, true, true);
        var connection = CreateHubConnection(userId.ToString());

        await connection.StartAsync();
        await Task.Delay(1000);

        await connection.StopAsync();
        await Task.Delay(1000);

        var scope =  _factory.Services.CreateScope();
        var rS = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var pS = scope.ServiceProvider.GetRequiredService<PlayerService>();
        var player = await pS.GetPlayerByUserId(userId);
        var room = await rS.GetRoomFromPlayerAsync(player.Id);
        
        room.Should().BeNull();
    }

    [Fact(Skip = "Implementieren sobald UpdateGameSettings fertig")]
    public async Task UpdateGameSettings_ShouldBroadcastSettings()
    {
    }

    [Fact(Skip = "Implementieren sobald StartGame fertig")]
    public async Task StartGame_ShouldBroadcastStart()
    {
    }
}