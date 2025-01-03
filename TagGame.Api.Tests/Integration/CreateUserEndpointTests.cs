using System.Drawing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Common;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Tests.Integration;

public class CreateUserEndpointTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> _factory;
    
    public CreateUserEndpointTests(WebApplicationFactory<Program> factory)
    {
        this._factory = factory;
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
        await context.Database.MigrateAsync();
    }
    
    [Fact]
    public async Task CreateUserAsync_ValidRequest_ReturnsUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = _fixture.Create<CreateUser.CreateUserRequest>();

        // Act
        var jsonResponse = await client.PostAsync(ApiRoutes.Initial.CreateUser, 
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        // Assert
        jsonResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var stringResponse = await jsonResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<Response<User>>(stringResponse, MappingOptions.JsonSerializerOptions);
        
        response.Should().NotBeNull();
        response.Value.Should().NotBeNull();
        response.Error.Should().BeNull();
        response.Value.DefaultName.Should().Be(request.Name);
        response.Value.DefaultAvatarColor.A.Should().Be(request.AvatarColor.A);
        response.Value.DefaultAvatarColor.R.Should().Be(request.AvatarColor.R);
        response.Value.DefaultAvatarColor.G.Should().Be(request.AvatarColor.G);
        response.Value.DefaultAvatarColor.B.Should().Be(request.AvatarColor.B);
    }

    [Fact]
    public async Task CreateUserAsync_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var client = _factory.CreateClient();

        var request = new CreateUser.CreateUserRequest
        {
            Name = string.Empty,
            AvatarColor = new ColorDTO()
            {
                A = Color.Blue.A,
                R = Color.Blue.R,
                G = Color.Blue.G,
                B = Color.Blue.B
            }
        };

        // Act
        var jsonResponse = await client.PostAsync(ApiRoutes.Initial.CreateUser, 
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        // Assert
        jsonResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var stringResponse = await jsonResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<Response<User>>(stringResponse, MappingOptions.JsonSerializerOptions);
        
        response.Should().NotBeNull();
        response.Error.Should().NotBeNull();
        response.Error.Message.Should().Contain("empty-name");
    }
}