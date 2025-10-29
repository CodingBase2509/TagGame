using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.DTOs.Auth;

#pragma warning disable CA1001

namespace TagGame.Api.Tests.Integration.Auth;

public sealed class AuthEndpointsTests : IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;

    public override async Task InitializeAsync()
    {
        UseDbTestContainer();
        await base.InitializeAsync();
        if (_dbContainer is not null)
        {
            var cs = await CreateDatabaseAsync("api_auth");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
            Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", cs);
        }

        _factory = new WebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        if ((await auth.Database.GetPendingMigrationsAsync()).Any())
            await auth.Database.MigrateAsync();
    }

    public override async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        await base.DisposeAsync();
    }

    [Fact]
    public async Task Initial_NewDevice_Creates_And_Returns_Tokens()
    {
        // Arrange
        var client = _factory!.CreateClient();
        var req = new InitialRequestDto { DeviceId = Guid.NewGuid().ToString("N"), DisplayName = "Test", AvatarColor = "#000000" };

        // Act
        var resp = await client.PostAsJsonAsync("/v1/auth/initial", req);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<InitialResponseDto>();
        body!.UserId.Should().NotBe(Guid.Empty);
        body.Tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Initial_ExistingDevice_Returns_Conflict()
    {
        // Arrange
        var client = _factory!.CreateClient();
        var device = Guid.NewGuid().ToString("N");

        // Act
        var first = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });

        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_UnknownDevice_Returns_NotFound()
    {
        // Arrange
        var client = _factory!.CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync("/v1/auth/login", new LoginRequestDto { DeviceId = "unknown-device" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_KnownDevice_Returns_Tokens()
    {
        // Arrange
        var client = _factory!.CreateClient();
        var device = Guid.NewGuid().ToString("N");

        // Act
        var init = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });
        init.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/v1/auth/login", new LoginRequestDto { DeviceId = device });

        // Assert
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await login.Content.ReadFromJsonAsync<LoginResponseDto>();
        body!.UserId.Should().NotBe(Guid.Empty);
        body.Tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_Rotates_Tokens()
    {
        // Arrange
        var client = _factory!.CreateClient();
        var device = Guid.NewGuid().ToString("N");

        // Act
        var init = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });
        init.EnsureSuccessStatusCode();
        var initBody = await init.Content.ReadFromJsonAsync<InitialResponseDto>();

        var refresh = await client.PostAsJsonAsync("/v1/auth/refresh", new RefreshRequestDto { RefreshToken = initBody!.Tokens.RefreshToken });

        // Assert
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var pair = await refresh.Content.ReadFromJsonAsync<RefreshResponseDto>();
        pair!.Tokens.RefreshToken.Should().NotBe(initBody.Tokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_Reuse_Returns_Unauthorized()
    {
        // Arrange
        var client = _factory!.CreateClient();
        var device = Guid.NewGuid().ToString("N");

        // Act
        var init = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });
        init.EnsureSuccessStatusCode();
        var initBody = await init.Content.ReadFromJsonAsync<InitialResponseDto>();

        var refresh1 = await client.PostAsJsonAsync("/v1/auth/refresh", new RefreshRequestDto { RefreshToken = initBody!.Tokens.RefreshToken });
        refresh1.EnsureSuccessStatusCode();

        var refreshReuse = await client.PostAsJsonAsync("/v1/auth/refresh", new RefreshRequestDto { RefreshToken = initBody.Tokens.RefreshToken });

        // Assert
        refreshReuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Is_Idempotent()
    {
        // Arrange
        var client = _factory!.CreateClient();
        var device = Guid.NewGuid().ToString("N");

        // Act
        var init = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });
        init.EnsureSuccessStatusCode();
        var initBody = await init.Content.ReadFromJsonAsync<InitialResponseDto>();

        var logout1 = await client.PostAsJsonAsync("/v1/auth/logout", new LogoutRequestDto { RefreshToken = initBody!.Tokens.RefreshToken });
        var logout2 = await client.PostAsJsonAsync("/v1/auth/logout", new LogoutRequestDto { RefreshToken = initBody.Tokens.RefreshToken });

        // Assert
        logout1.StatusCode.Should().Be(HttpStatusCode.OK);
        logout2.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
