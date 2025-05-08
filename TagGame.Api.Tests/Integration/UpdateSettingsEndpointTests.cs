using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
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
using TagGame.Shared.DTOs.Common;

namespace TagGame.Api.Tests.Integration;

public class UpdateSettingsEndpointTests  : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> _factory;
    private const string route = $"{ApiRoutes.GameRoom.GroupName}{ApiRoutes.GameRoom.UpdateSettings}";

    public UpdateSettingsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        UseDbTestContainer();
    }

    string GetRoute(Guid roomId) => route.Replace("{roomId:guid}", roomId.ToString());

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
        await context.Database.MigrateAsync();
    }

    [Fact(Skip = "Implemented in Lobbyy Hub")]
    public async Task UpdateSettings_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        
        var room = await roomService.CreateAsync(Guid.NewGuid(), "Test Room");
        var user = await userService.AddUserAsync("Test User", ColorDTO.FromArgb(13,153,153,159));
        var client = _factory.CreateClient();
        
        var validRequest = _fixture.Build<GameSettings>()
            .With(x => x.RoomId, room.Id)
            .Create();
        
        var authHeader = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.Id.ToString()));
        
        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        var response = await client.PutAsync(GetRoute(room.Id),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));
        
        // Assert
         response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Implemented in Lobbyy Hub")]
    public async Task UpdateSettings_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        
        var user = await userService.AddUserAsync("Test User", ColorDTO.FromArgb(13,153,153,159));
        var client = _factory.CreateClient();
        var invalidRequest = _fixture.Build<GameSettings>()
            .With(x => x.RoomId, Guid.Empty)
            .Create();
        
        var authHeader = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.Id.ToString()));
        
        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        var response = await client.PutAsync(GetRoute(Guid.NewGuid()),
            new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json"));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("empty-id");
    }

    [Fact(Skip = "Implemented in Lobbyy Hub")]
    public async Task UpdateSettings_ServiceFailsToUpdateSettings_ReturnsServerError()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        
        var room = await roomService.CreateAsync(Guid.NewGuid(), "Test Room");
        var user = await userService.AddUserAsync("Test User", ColorDTO.FromArgb(13,153,153,159));

        var dbMock = Utility.CreateDbMockForRooms();
        dbMock.Setup(x => x.Users.GetByIdAsync(user.Id, It.IsAny<bool>()))
            .ReturnsAsync(user);
        dbMock.Setup(x => x.Rooms.GetByIdAsync(room.Id, It.IsAny<bool>()))
            .ReturnsAsync(room);
        dbMock.Setup(x => x.Rooms.Include(It.IsAny<Expression<Func<GameRoom, GameSettings>>>()))
            .Returns(dbMock.Object.Rooms);
        dbMock.Setup(x => x.Settings.UpdateAsync(It.IsAny<GameSettings>()))
            .ReturnsAsync(false);
        
        var validRequest = _fixture.Build<GameSettings>()
            .With(x => x.RoomId, room.Id)
            .Create();
        
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped<IDataAccess>(_ => dbMock.Object));
        }).CreateClient();
        
        var authHeader = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.Id.ToString()));
        
        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        var response = await client.PutAsync(GetRoute(room.Id),
            new StringContent(JsonSerializer.Serialize(validRequest), Encoding.UTF8, "application/json"));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);
        
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-updated-settings");
    }
}