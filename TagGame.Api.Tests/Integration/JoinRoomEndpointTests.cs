using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Common;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Tests.Integration;

public class JoinRoomEndpointTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> _factory;
    private const string route = $"{ApiRoutes.GameRoom.GroupName}{ApiRoutes.GameRoom.JoinRoom}";

    public JoinRoomEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        UseDbTestContainer();
    }
    
    string GetRoute(Guid roomid) => route.Replace("{roomId:guid}", roomid.ToString());
    
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
    
    [Fact]
    public async Task JoinRoom_ValidRequest_ReturnsRoomAndPlayerDetails()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        
        var room = await roomService.CreateAsync(Guid.NewGuid(), "Test Room");
        var user = await userService.AddUserAsync("Test User", ColorDTO.FromArgb(13,153,153,159));
        var validRequest = new JoinGameRoom.JoinGameRoomRequest
        {
            UserId = user.Id,
            GameName = room.Name,
            AccessCode = room.AccessCode
        };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync(GetRoute(room.Id),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<JoinGameRoom.JoinGameRoomResponse>>(
            stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Value.Room.Id.Should().Be(room.Id);
        result.Value.PlayerId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task JoinRoom_ValidRequest_JoinsRoomWithExistingPlayer_ReturnRoomAndPlayerDetails()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        
        var room = await roomService.CreateAsync(Guid.NewGuid(), "Test Room");
        var user = await userService.AddUserAsync("Test User", ColorDTO.FromArgb(13,153,153,159));
        var player = await playerService.CreatePlayerAsync(user.Id);
        
        var validRequest = new JoinGameRoom.JoinGameRoomRequest
        {
            UserId = user.Id,
            GameName = room.Name,
            AccessCode = room.AccessCode
        };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync(GetRoute(room.Id),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<JoinGameRoom.JoinGameRoomResponse>>(
            stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Value.Room.Id.Should().Be(room.Id);
        result.Value.PlayerId.Should().Be(player.Id);
    }

    [Fact]
    public async Task JoinRoom_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidRequest = new JoinGameRoom.JoinGameRoomRequest
        {
            UserId = Guid.Empty, // Invalid UserId
            GameName = string.Empty, // Invalid Game Name
            AccessCode = string.Empty // Invalid Access Code
        };

        // Act
        var response = await client.PostAsync(GetRoute(Guid.NewGuid()),
            new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Error.Message.Should().Contain("empty-name");
        result.Error.Message.Should().Contain("empty-accesscode");
    }

    [Fact]
    public async Task JoinRoom_RoomNotFound_ReturnsNotFound()
    {
        // Arrange
        var validRequest = new JoinGameRoom.JoinGameRoomRequest
        {
            UserId = Guid.NewGuid(),
            GameName = "Nonexistent",
            AccessCode = "12345"
        };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync(GetRoute(Guid.NewGuid()),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-found-room");
    }

    [Fact]
    public async Task JoinRoom_ServiceFailsToCreatePlayer_ReturnsServerError()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        
        var room = await roomService.CreateAsync(Guid.NewGuid(), "Test Room");
        
        var validRequest = new JoinGameRoom.JoinGameRoomRequest
        {
            UserId = Guid.NewGuid(),
            GameName = room.Name,
            AccessCode = room.AccessCode
        };
        
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync(GetRoute(room.Id),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-created-player");
    }

    [Fact]
    public async Task JoinRoom_ServiceFailsToAddPlayerToRoom_ReturnsServerError()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var room = await roomService.CreateAsync(Guid.NewGuid(), "Test Room");
        
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            
        var validRequest = new JoinGameRoom.JoinGameRoomRequest
        {
            UserId = Guid.NewGuid(),
            GameName = room.Name,
            AccessCode = room.AccessCode
        };
        
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync(GetRoute(room.Id),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-created-player"/*"player-not-joined-room"*/);
    }
}