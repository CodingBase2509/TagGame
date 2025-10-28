using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using TagGame.Api.Core.Features.Auth;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Tests.Unit.Auth;

public sealed class AuthTokenHelperTests
{
    private static JwtOptions CreateOptions() => new()
    {
        Issuer = "TagGame",
        Audience = "TagGameClient",
        SigningKey = "unit-test-signing-key-1234567890",
        AccessMinutes = 20,
        RefreshDays = 14
    };

    [Fact]
    public void CreateJwtToken_Generates_Valid_Token_With_Claims()
    {
        // Arrange
        var opts = CreateOptions();
        var user = new User { Id = Guid.NewGuid(), DisplayName = "Tester" };
        var now = DateTimeOffset.UnixEpoch.AddDays(10);

        // Act
        var (token, exp) = AuthTokenHelper.CreateJwtToken(user, now, opts);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        exp.Should().BeCloseTo(now.AddMinutes(opts.AccessMinutes), TimeSpan.FromSeconds(1));

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey));
        var result = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = opts.Issuer,
            ValidAudience = opts.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(30)
        }, out _);

        result.FindFirst(JwtRegisteredClaimNames.Sub)!.Value.Should().Be(user.Id.ToString());
        result.FindFirst("nickname").Should().NotBeNull();
    }

    [Fact]
    public void CreateRefreshToken_Generates_Raw_And_Hash_With_Expiry_And_Family()
    {
        // Arrange
        var opts = CreateOptions();
        var now = DateTimeOffset.UnixEpoch.AddDays(10);

        // Act
        var (raw, hash, exp, family) = AuthTokenHelper.CreateRefreshToken(null, now, opts);

        // Assert
        raw.Should().NotBeNullOrWhiteSpace();
        hash.Should().MatchRegex("^[a-f0-9]{64}$");
        exp.Should().Be(now.AddDays(opts.RefreshDays));
        family.Should().NotBe(Guid.Empty);

        // Reuse existing family id
        var fam2 = Guid.NewGuid();
        var (_, _, _, famRet) = AuthTokenHelper.CreateRefreshToken(fam2, now, opts);
        famRet.Should().Be(fam2);
    }

    [Fact]
    public void HashRefreshToken_Is_Deterministic_And_Lowercase_Hex()
    {
        // Arrange
        var fixture = new Fixture();
        var raw = fixture.Create<string>();

        // Act
        var h1 = AuthTokenHelper.HashRefreshToken(raw);
        var h2 = AuthTokenHelper.HashRefreshToken(raw);

        // Assert
        h1.Should().Be(h2);
        h1.Should().MatchRegex("^[a-f0-9]{64}$");
    }

    [Fact]
    public void EnsureToken_Throws_For_Invalid_States_And_Allows_Valid()
    {
        // Arrange
        var now = DateTimeOffset.UnixEpoch.AddDays(42);
        RefreshToken? missing = null;
        var revoked = new RefreshToken { RevokedAt = now };
        var expired = new RefreshToken { ExpiresAt = now.AddSeconds(-1) };
        var ok = new RefreshToken { ExpiresAt = now.AddSeconds(10) };

        // Act / Assert
        Action a1 = () => AuthTokenHelper.EnsureToken(missing, now);
        Action a2 = () => AuthTokenHelper.EnsureToken(revoked, now);
        Action a3 = () => AuthTokenHelper.EnsureToken(expired, now);
        Action a4 = () => AuthTokenHelper.EnsureToken(ok, now);

        a1.Should().Throw<InvalidOperationException>();
        a2.Should().Throw<InvalidOperationException>();
        a3.Should().Throw<InvalidOperationException>();
        a4.Should().NotThrow();
    }
}
