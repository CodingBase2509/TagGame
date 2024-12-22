using System.Net;
using System.Net.Http.Headers;
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

public class GetRoomEndpointTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> _factory;
    private const string route = $"{ApiRoutes.GameRoom.GroupName}{ApiRoutes.GameRoom.GetRoom}";

    public GetRoomEndpointTests(WebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task GetRoom_ValidRoomId_ReturnsRoomDetails()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var roomService = scope.ServiceProvider.GetRequiredService<GameRoomService>();
        
        var user = await userService.AddUserAsync("Test", ColorDTO.FromArgb(124, 53, 61, 15));
        var createdRoom = await roomService.CreateAsync(user.Id, "Test Room");

        var client = _factory.CreateClient();
        var get_route = GetRoute(createdRoom.Id);
        var authHeader = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.Id.ToString()));
        
        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        var response = await client.GetAsync(get_route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<GameRoom>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Value.Id.Should().Be(createdRoom.Id);
        result.Value.Name.Should().Be("Test Room");
    }

    [Fact]
    public async Task GetRoom_NonexistentRoomId_ReturnsNotFound()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var user = await userService.AddUserAsync("Test", ColorDTO.FromArgb(124, 53, 61, 15));
        
        var client = _factory.CreateClient();
        var get_route = GetRoute(Guid.NewGuid());
        var authHeader = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.Id.ToString()));
        
        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        var response = await client.GetAsync(get_route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response<Error>>(stringResponse, MappingOptions.JsonSerializerOptions);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("not-found-room");
    }
}
