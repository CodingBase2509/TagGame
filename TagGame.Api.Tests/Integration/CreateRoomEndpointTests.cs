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

public class CreateRoomEndpointTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> _factory;
    private const string route = $"{ApiRoutes.GameRoom.GroupName}{ApiRoutes.GameRoom.CreateRoom}";

    public CreateRoomEndpointTests(WebApplicationFactory<Program> factory)
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
    
    [Fact]
    public async Task CreateRoom_ValidRequest_ReturnsRoomDetails()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var user = await userService.AddUserAsync("Test", ColorDTO.FromArgb(255, 0, 0, 255));
        
        var client = _factory.CreateClient();
        var validRequest = new CreateGameRoom.CreateGameRoomRequest
        {
            UserId = user.Id,
            GameRoomName = "Test Room"
        };

        // Act
        var response = await client.PostAsync(route,
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<CreateGameRoom.CreateGameRoomResponse>>(
            stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Value.RoomId.Should().NotBeEmpty();
        result.Value.RoomName.Should().Be("Test Room");
        result.Value.AccessCode.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateRoom_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidRequest = new CreateGameRoom.CreateGameRoomRequest
        {
            UserId = Guid.Empty, // Invalid UserId
            GameRoomName = string.Empty // Invalid Room Name
        };

        // Act
        var response = await client.PostAsync(route,
            new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Error.Message.Should().Contain("empty-id");
        result.Error.Message.Should().Contain("empty-name");
    }

    [Fact]
    public async Task CreateRoom_ServiceFailsToCreateRoom_ReturnsServerError()
    {
        // Arrange
        var validRequest = new CreateGameRoom.CreateGameRoomRequest
        {
            UserId = Guid.NewGuid(),
            GameRoomName = "Test Room"
        };

        var dbMock = Utility.CreateDbMockForRooms();
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            services.AddScoped<IDataAccess>(_ => dbMock.Object));
        }).CreateClient();

        // Act
        var response = await client.PostAsync(route,
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-created-room");
    }
    
    [Fact]
    public async Task CreateRoom_ServiceFailsToCreatePlayer_ReturnsServerError()
    {
        // Arrange
        var validRequest = new CreateGameRoom.CreateGameRoomRequest
        {
            UserId = Guid.NewGuid(),
            GameRoomName = "Test Room"
        };

        var dbMock = Utility.CreateDbMockForRooms();
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var userMock = new Mock<IDataSet<User>>();
        dbMock.Setup(x => x.Users).Returns(userMock.Object);
        
        userMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);
        
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped<IDataAccess>(_ => dbMock.Object));
        }).CreateClient();

        // Act
        var response = await client.PostAsync(route,
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-created-player");
    }
    
    [Fact]
    public async Task CreateRoom_ServiceFailsToAddPlayerToRoom_ReturnsServerError()
    {
        // Arrange
        var validRequest = new CreateGameRoom.CreateGameRoomRequest
        {
            UserId = Guid.NewGuid(),
            GameRoomName = "Test Room"
        };

        var dbMock = Utility.CreateDbMockForPlayers();
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped<IDataAccess>(_ => dbMock.Object));
        }).CreateClient();

        // Act
        var response = await client.PostAsync(route,
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("player-not-joined-room");
    }
}