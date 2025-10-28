using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Tests.Unit.Auth;

public sealed class AuthTokenServiceTests
{
    private static JwtOptions CreateOptions() => new()
    {
        Issuer = "TagGame",
        Audience = "TagGameClient",
        SigningKey = "unit-test-signing-key-1234567890",
        AccessMinutes = 10,
        RefreshDays = 7
    };

    private static (AuthDbContext db, IAuthTokenService svc, DateTimeOffset now) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AuthDbContext(options);
        var now = DateTimeOffset.UtcNow;
        var tp = new FixedTimeProvider(now);
        var logger = new Mock<ILogger<AuthTokenService>>().Object;

        var svc = new AuthTokenService(db, Options.Create(CreateOptions()), tp, logger);
        return (db, svc, now);
    }

    [Fact]
    public async Task IssueTokenAsync_Creates_Refresh_Row_And_Returns_Pair()
    {
        // Arrange
        var (db, svc, now) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), DisplayName = "Zeus", CreatedAt = now };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Act
        var pair = await svc.IssueTokenAsync(user);

        // Assert
        pair.AccessToken.Should().NotBeNullOrWhiteSpace();
        pair.RefreshToken.Should().NotBeNullOrWhiteSpace();
        pair.AccessExpiresAt.Should().BeCloseTo(now.AddMinutes(CreateOptions().AccessMinutes), TimeSpan.FromSeconds(1));
        pair.RefreshExpiresAt.Should().BeCloseTo(now.AddDays(CreateOptions().RefreshDays), TimeSpan.FromSeconds(1));

        var saved = db.RefreshTokens.Single();
        saved.UserId.Should().Be(user.Id);
        saved.FamilyId.Should().NotBe(Guid.Empty);
        saved.RevokedAt.Should().BeNull();
        saved.ReplacedById.Should().BeNull();
        saved.CreatedAt.Should().Be(now);
        saved.ExpiresAt.Should().Be(pair.RefreshExpiresAt);

        // Validate JWT
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CreateOptions().SigningKey));
        var principal = handler.ValidateToken(pair.AccessToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = CreateOptions().Issuer,
            ValidAudience = CreateOptions().Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(30)
        }, out _);
        principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!.Value
            .Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task RefreshTokenAsync_Rotates_And_Revokes_Old()
    {
        // Arrange
        var (db, svc, now) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = now };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var first = await svc.IssueTokenAsync(user);

        // Act
        var second = await svc.RefreshTokenAsync(first.RefreshToken);

        // Assert
        second.RefreshToken.Should().NotBe(first.RefreshToken);
        second.AccessToken.Should().NotBe(first.AccessToken);

        var tokens = db.RefreshTokens.OrderBy(x => x.CreatedAt).ToList();
        tokens.Count.Should().Be(2);
        var old = tokens[0];
        var newer = tokens[1];

        old.RevokedAt.Should().Be(now);
        old.ReplacedById.Should().Be(newer.Id);
        newer.FamilyId.Should().Be(old.FamilyId);
        newer.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_Throws()
    {
        // Arrange
        var (db, svc, _) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Act
        Func<Task> act = () => svc.RefreshTokenAsync("does-not-exist");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid refresh token.*");
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_Throws()
    {
        // Arrange
        var (db, svc, _) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var issued = await svc.IssueTokenAsync(user);
        await svc.RevokeTokenAsync(issued.RefreshToken);

        // Act
        Func<Task> act = () => svc.RefreshTokenAsync(issued.RefreshToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Refresh token has been revoked.*");
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_Throws()
    {
        // Arrange
        var (db, svc, now) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = now };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var pair = await svc.IssueTokenAsync(user);

        // Expire the stored token
        var rt = db.RefreshTokens.Single();
        rt.ExpiresAt = now.AddSeconds(-1);
        await db.SaveChangesAsync();

        // Act
        Func<Task> act = () => svc.RefreshTokenAsync(pair.RefreshToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Refresh token has expired.*");
    }

    [Fact]
    public async Task IssueTokenAsync_EmptyUserId_Throws()
    {
        // Arrange
        var (db, svc, now) = CreateSut();
        var user = new User { Id = Guid.Empty, CreatedAt = now };

        // Act
        Func<Task> act = () => svc.IssueTokenAsync(user);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User must have an Id*");
    }

    [Fact]
    public async Task RevokeTokenAsync_Is_Idempotent()
    {
        // Arrange
        var (db, svc, now) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = now };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var issued = await svc.IssueTokenAsync(user);

        // Act
        await svc.RevokeTokenAsync(issued.RefreshToken);
        await svc.RevokeTokenAsync(issued.RefreshToken); // second time, no throw

        // Assert
        var token = db.RefreshTokens.Single();
        token.RevokedAt.Should().Be(now);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FixedTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }
}
