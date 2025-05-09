using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        else
        {
            var data = scope.ServiceProvider.GetRequiredService<IDataAccess>();
            room = data.Rooms
                .Include(r => r.Players)
                .Where(r => r.Players.Any())
                .FirstOrDefault();
        }
        
        if (addPlayerToRoom && room is not null && player is not null)
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
    
        var userId2 = await CreateEntities(true, false, true);
        var connection2 = CreateHubConnection(userId2.ToString());

        var tcs = new TaskCompletionSource<PlayerLeftGameInfo>();
        connection1.On<PlayerLeftGameInfo>("ReceivePlayerLeft", info => {
            tcs.SetResult(info);
            return Task.CompletedTask;
        });

        await connection1.StartAsync();
        await connection2.StartAsync();
        await Task.Delay(1000);

        await connection2.StopAsync();

        // wait on ReceivePlayerLeft (max. 4 seconds)
        var leftInfo = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        leftInfo.Should().NotBeNull();
        leftInfo.DisconnectType.Should().Be(PlayerDisconnectType.LeftGame);

        await connection1.StopAsync();
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
        var data = scope.ServiceProvider.GetRequiredService<IDataAccess>();
        var players = data.Players.Where(p => true)
            .ToList();
        var rooms = data.Rooms.Where(r => true)
            .ToList();
        
        players.Should().NotBeNull();
        players.Should().BeEmpty();
        
        rooms.Should().NotBeNull();
        rooms.Should().BeEmpty();
        
    }
    
    [Fact]
    public async Task Disconnect_ShouldNotDeleteRoom_WhenOtherPlayersExist()
    {
        var userId1 = await CreateEntities(true, true, true);
        var connection1 = CreateHubConnection(userId1.ToString());

        var userId2 = await CreateEntities(true, true, true);
        var connection2 = CreateHubConnection(userId2.ToString());

        await connection1.StartAsync();
        await connection2.StartAsync();
        await Task.Delay(1000);

        // Disconnect connection2 (one of the two players)
        await connection2.StopAsync();
        await Task.Delay(1000);

        // Room should still exist (connection1 still in)
        using var scope = _factory.Services.CreateScope();
        var rS = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var player = await scope.ServiceProvider
            .GetRequiredService<PlayerService>()
            .GetPlayerByUserId(userId1);
        var room = await rS.GetRoomFromPlayerAsync(player?.Id ?? Guid.Empty);
    
        room.Should().NotBeNull();

        // Cleanup
        await connection1.StopAsync();
        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
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