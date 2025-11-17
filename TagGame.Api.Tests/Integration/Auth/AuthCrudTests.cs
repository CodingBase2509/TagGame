using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;
using Xunit.Abstractions;

namespace TagGame.Api.Tests.Integration.Auth;

public sealed class AuthCrudTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase
{
    private IFixture _fixture = null!;

    public override async Task InitializeAsync()
    {
        if (!DockerRequirement.IsAvailable)
            return;
        UseDbTestContainer();
        await base.InitializeAsync();
        var cs = await CreateDatabaseAsync("authcrud");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
        Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", cs);

        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        await using var ctx = new DesignTimeAuthFactory().CreateDbContext([]);
        await ctx.Database.MigrateAsync();
    }

    [DockerFact]
    public async Task Migrations_Apply_And_BasicCrud_Works()
    {
        await using var ctx = new DesignTimeAuthFactory().CreateDbContext([]);

        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.Id, Guid.NewGuid())
            .With(u => u.DisplayName, "Test User")
            .With(u => u.Email, (string?)null)
            .With(u => u.DeviceId, "device-123")
            .With(u => u.AvatarColor, "#FFAA00")
            .With(u => u.Flags, 0)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow)
            .With(u => u.LastSeenAt, (DateTimeOffset?)null)
            .Create();

        ctx.Users.Add(user);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save user failed: {ex}");
            throw;
        }

        var token1 = _fixture.Build<RefreshToken>()
            .With(t => t.Id, Guid.NewGuid())
            .With(t => t.UserId, user.Id)
            .With(t => t.FamilyId, Guid.NewGuid())
            .With(t => t.TokenHash, Guid.NewGuid().ToString("N"))
            .With(t => t.CreatedAt, DateTimeOffset.UtcNow)
            .With(t => t.ExpiresAt, DateTimeOffset.UtcNow.AddDays(7))
            .With(t => t.RevokedAt, (DateTimeOffset?)null)
            .With(t => t.ReplacedById, (Guid?)null)
            .Create();

        ctx.RefreshTokens.Add(token1);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save token1 failed: {ex}");
            throw;
        }

        var token2 = _fixture.Build<RefreshToken>()
            .With(t => t.Id, Guid.NewGuid())
            .With(t => t.UserId, user.Id)
            .With(t => t.FamilyId, token1.FamilyId)
            .With(t => t.TokenHash, Guid.NewGuid().ToString("N"))
            .With(t => t.CreatedAt, DateTimeOffset.UtcNow)
            .With(t => t.ExpiresAt, DateTimeOffset.UtcNow.AddDays(7))
            .With(t => t.RevokedAt, (DateTimeOffset?)null)
            .With(t => t.ReplacedById, (Guid?)null)
            .Create();

        // Act
        ctx.RefreshTokens.Add(token2);
        token1.RevokedAt = DateTimeOffset.UtcNow;
        token1.ReplacedById = token2.Id;
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save token2 failed: {ex}");
            throw;
        }

        var tokensForUser = ctx.RefreshTokens.Where(t => t.UserId == user.Id).ToList();

        // Assert
        tokensForUser.Count.Should().Be(2);
        tokensForUser.Should().ContainSingle(t => t.Id == token1.Id)
            .Which.Should().Match<RefreshToken>(t => t.RevokedAt.HasValue && t.ReplacedById == token2.Id);
        tokensForUser.Should().ContainSingle(t => t.Id == token2.Id)
            .Which.RevokedAt.Should().BeNull();
    }

    [DockerFact]
    public async Task Deleting_User_Cascades_To_RefreshTokens()
    {
        await using var ctx = new DesignTimeAuthFactory().CreateDbContext([]);

        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            LastSeenAt = null,
            DisplayName = "User",
            Email = null,
            DeviceId = "dev-1",
            AvatarColor = "#000000",
            Flags = 0
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = Guid.NewGuid(),
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        ctx.RefreshTokens.Add(token);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save token failed: {ex}");
            throw;
        }

        // Act
        ctx.Users.Remove(user);
        await ctx.SaveChangesAsync();

        // Assert
        ctx.RefreshTokens.Count(t => t.UserId == user.Id).Should().Be(0);
    }
}

// Fallback Docker wrapper removed; using IntegrationTestBase + Testcontainers.PostgreSql.
